using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MiSharp.Translator.Abstract
{
    public abstract class Generator
    {
        protected abstract ITypeBuilder TypeBuilder { get; }

        protected abstract IPredefinedTypes PredefinedTypes { get; }

        protected abstract IPropertyGenerator PropertyGenerator { get; }

        protected abstract IMethodGenerator MethodGenerator { get; }

        protected abstract IConstructorGenerator ConstructorGenerator { get; }

        protected abstract IStatementGenerator StatementGenerator { get; }

        protected abstract IExpressionGenerator ExpressionGenerator { get; }

        protected abstract ITypeReferenceGenerator TypeReferenceGenerator { get; }

        protected abstract ITypeReferenceBuilder TypeReferenceBuilder { get; }

        protected abstract IGenericTypeReferenceBuilder GenericTypeReferenceBuilder { get; }

        public void Generate(string projectPath, string outputPath, IEnumerable<string> assembliesPaths)
        {
            var collector = new CSharpProjectSourcesCollector();
            var sources = collector.CollectProjectSources(projectPath);
            if (!string.IsNullOrEmpty(outputPath))
            {
                if (Directory.Exists(outputPath))
                {
                    Directory.Delete(outputPath, true);
                }
                Directory.CreateDirectory(outputPath);
            }
            Generate(sources, outputPath, assembliesPaths.ToList());
        }

        private void Generate(IEnumerable<SourceFile> sources, string outputPath, List<string> assembliesPaths)
        {
            var syntaxTrees = new List<SyntaxTree>();
            foreach (var source in sources)
            {
                SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(source.Text, path: source.Path);
                

                syntaxTrees.Add(tree);
            }

            var mscorlib = MetadataReference.CreateFromFile((typeof(object).Assembly.Location));
            var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

            CSharpCompilation compilation = CSharpCompilation.Create("App")
                .AddSyntaxTrees(syntaxTrees)
                .AddReferences(mscorlib, systemCore)
                .AddReferences(assembliesPaths.Select(assemblyPath => MetadataReference.CreateFromFile(assemblyPath)));

            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree, false);
                Generate(syntaxTree, semanticModel, outputPath);
            }
        }

        private void Generate(SyntaxTree syntaxTree, SemanticModel semanticModel, string outputPath)
        {
            var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classDeclaration in classDeclarations)
            {
                Generate(classDeclaration, semanticModel, outputPath);
            }
            var interfaceDeclarations = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            foreach (var interfaceDeclaration in interfaceDeclarations)
            {
                Generate(interfaceDeclaration, semanticModel, outputPath);
            }
            var enumDeclarations = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
            foreach (var enumDeclaration in enumDeclarations)
            {
                Generate(enumDeclaration, semanticModel, outputPath);
            }
        }

        private void Generate(InterfaceDeclarationSyntax interfaceDeclaration, SemanticModel semanticModel, string outputPath)
        {
            INamedTypeSymbol typeInfo = semanticModel.GetDeclaredSymbol(interfaceDeclaration);
            var fullyQualifiedNameParts = SyntaxTreeHelper.GetFullyQualifiedNameParts(typeInfo);
            var genericTypeParameters = typeInfo.TypeParameters;
            var typeReferences = new HashSet<string>();

            var propertyDeclarations = interfaceDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            var generatedProperties = GetProperties(semanticModel, typeReferences, propertyDeclarations, true);

            var methodDeclarations = interfaceDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var generatedMethods = GetMethods(semanticModel, typeReferences, methodDeclarations);

            var interfaceSourceCode = TypeBuilder.BuildInterface(semanticModel,
                fullyQualifiedNameParts, genericTypeParameters,
                generatedProperties,
                generatedMethods);

            WriteToOutputIfPathPresent(outputPath, fullyQualifiedNameParts, interfaceSourceCode.MainPart);
        }

        private void Generate(EnumDeclarationSyntax enumDeclaration, SemanticModel semanticModel, string outputPath)
        {
            INamedTypeSymbol typeInfo = semanticModel.GetDeclaredSymbol(enumDeclaration);
            var fullyQualifiedNameParts = SyntaxTreeHelper.GetFullyQualifiedNameParts(typeInfo);

            var sourceCode = TypeBuilder.BuildEnum(semanticModel, fullyQualifiedNameParts, enumDeclaration.Members);

            WriteToOutputIfPathPresent(outputPath, fullyQualifiedNameParts, sourceCode.MainPart);
        }

        private void Generate(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, string outputPath)
        {
            var nativeImplementationAttribute =
                SyntaxTreeHelper.GetNativeImplementationAttribute(classDeclaration);
            if (!nativeImplementationAttribute.HasValue)
            {
                GenerateOrdinaryClass(classDeclaration, semanticModel, outputPath);
            }
            else
            {
                ProcessNativeImplementationClass(classDeclaration, semanticModel, outputPath);
            }
        }

        private void ProcessNativeImplementationClass(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, string outputPath)
        {
            //throw new NotImplementedException();
        }

        private void GenerateOrdinaryClass(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, string outputPath)
        {
            INamedTypeSymbol typeInfo = semanticModel.GetDeclaredSymbol(classDeclaration);
            var fullyQualifiedNameParts = SyntaxTreeHelper.GetFullyQualifiedNameParts(typeInfo);
            var genericTypeParameters = typeInfo.TypeParameters;
            var typeReferences = new HashSet<string>();

            var propertyDeclarations = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            var generatedProperties = GetProperties(semanticModel, typeReferences, propertyDeclarations, false);

            var fieldsDeclarations = classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>();
            var generatedFields = GetFields(semanticModel, typeReferences, fieldsDeclarations);

            var constructorDeclarations = classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            var generatedConstructors = GetConstructors(semanticModel, typeReferences, constructorDeclarations);

            var methodDeclarations = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var generatedMethods = GetMethods(semanticModel, typeReferences, methodDeclarations);

            var classSourceCode = TypeBuilder.BuildType(semanticModel,
                fullyQualifiedNameParts, genericTypeParameters,
                typeInfo.BaseType,
                typeInfo.AllInterfaces,
                generatedConstructors,
                generatedFields,
                generatedProperties,
                generatedMethods);

            WriteToOutputIfPathPresent(outputPath, fullyQualifiedNameParts, classSourceCode.MainPart);
        }

        private void WriteToOutputIfPathPresent(string outputPath, string[] fullyQualifiedNameParts, string sourceCodeText)
        {
            if (!string.IsNullOrEmpty(outputPath))
            {
                string className = fullyQualifiedNameParts.Last();
                string path = "";
                for (int i = 0; i < fullyQualifiedNameParts.Length - 1; i++)
                {
                    path += fullyQualifiedNameParts[i].ToLower() + "\\";
                }
                path += className;
                path = outputPath + "\\" + path.TrimEnd('\\') + ".java";
                new FileInfo(path).Directory.Create();
                File.WriteAllText(path, sourceCodeText);
            }
        }

        private SourceCode GetMethods(SemanticModel semanticModel, HashSet<string> typeReferences, IEnumerable<MethodDeclarationSyntax> methodDeclarations)
        {
            var generatedMethods = new SourceCode { MainPart = "" };
            foreach (var methodDeclaration in methodDeclarations)
            {
                string identifier = methodDeclaration.Identifier.ValueText;
                var returnType = methodDeclaration.ReturnType;
                var returnTypeReference = TypeReferenceGenerator.GenerateTypeReference(returnType, semanticModel);
                if (!returnTypeReference.IsPredefined)
                {
                    typeReferences.Add(returnTypeReference.Text);
                }
                var typeParameters = new List<string>();
                if (methodDeclaration.TypeParameterList != null)
                {
                    foreach (var typeParameter in methodDeclaration.TypeParameterList.Parameters)
                    {
                        typeParameters.Add(typeParameter.Identifier.ValueText);
                    }
                }
                var parameters = new List<Var>();
                foreach (var parameter in methodDeclaration.ParameterList.Parameters)
                {
                    var typeReference = TypeReferenceGenerator.GenerateTypeReference(parameter.Type, semanticModel);
                    parameters.Add(new Var
                    {
                        Name = parameter.Identifier.ValueText,
                        Type = typeReference
                    });

                }
                var accessModifier = GetAccessModifier(methodDeclaration.Modifiers.ToList());
                bool isStatic = HasStaticModifier(methodDeclaration.Modifiers.ToList());
                Optional<List<string>> body;
                if (methodDeclaration.Body == null)
                {
                    body = new Optional<List<string>>();
                }
                else
                {
                    var statements = new List<string>();
                    foreach (var statement in methodDeclaration.Body.Statements)
                    {
                        string generatedStatement = StatementGenerator.Generate(statement, semanticModel);
                        statements.Add(generatedStatement);
                        new Optional<List<string>>();
                    }
                    body = new Optional<List<string>>(statements);
                }

                bool isVirtual = !body.HasValue || HasVirtualModifier(methodDeclaration.Modifiers.ToList());

                var generatedMethod = MethodGenerator.Generate(identifier,
                    returnTypeReference,
                    typeParameters,
                    accessModifier,
                    parameters,
                    body,
                    isStatic,
                    isVirtual,
                    semanticModel);
                generatedMethods.MainPart += "\n" + generatedMethod.MainPart;
            }
            return generatedMethods;
        }

        private SourceCode GetConstructors(SemanticModel semanticModel, HashSet<string> typeReferences, IEnumerable<ConstructorDeclarationSyntax> constructorDeclarations)
        {
            var generatedConstructors = new SourceCode { MainPart = "" };
            foreach (var constructorDeclaration in constructorDeclarations)
            {
                string identifier = constructorDeclaration.Identifier.ValueText;
                var parameters = new List<Var>();
                foreach (var parameter in constructorDeclaration.ParameterList.Parameters)
                {
                    var typeReference = TypeReferenceGenerator.GenerateTypeReference(parameter.Type, semanticModel);
                    parameters.Add(new Var
                    {
                        Name = parameter.Identifier.ValueText,
                        Type = typeReference
                    });

                }
                var constructorAccessModifier = GetAccessModifier(constructorDeclaration.Modifiers.ToList());
                var statements = new List<string>();
                foreach (var statement in constructorDeclaration.Body.Statements)
                {
                    string generatedStatement = StatementGenerator.Generate(statement, semanticModel);
                    statements.Add(generatedStatement);

                }
                var generatedConstructor = ConstructorGenerator.Generate(identifier, constructorAccessModifier, parameters,
                    statements, semanticModel);
                generatedConstructors.MainPart += "\n" + generatedConstructor.MainPart;
            }
            return generatedConstructors;
        }

        private SourceCode GetProperties(SemanticModel semanticModel, HashSet<string> typeReferences,
            IEnumerable<PropertyDeclarationSyntax> propertyDeclarations, bool isFromInterface)
        {
            var generatedProperties = new SourceCode { MainPart = "" };
            foreach (var propertyDeclaration in propertyDeclarations)
            {
                string identifier = propertyDeclaration.Identifier.ValueText;
                var propertyType = propertyDeclaration.Type;
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(propertyType, semanticModel);
                if (!typeReference.IsPredefined)
                {
                    typeReferences.Add(typeReference.Text);
                }

                bool isStatic = HasStaticModifier(propertyDeclaration.Modifiers.ToList());
                bool isVirtual = isFromInterface || HasVirtualModifier(propertyDeclaration.Modifiers.ToList());

                var propertyAccessModifier = GetAccessModifier(propertyDeclaration.Modifiers.ToList());
                var accessors = propertyDeclaration.AccessorList.Accessors;
                var getAccessor = accessors.FirstOrDefault(syntax => syntax.Keyword.Text == "get");
                var setAccessor = accessors.FirstOrDefault(syntax => syntax.Keyword.Text == "set");

                if (propertyDeclaration.AccessorList.Accessors.Any(syntax => syntax.Body != null))
                {
                    var complexPropertyDescription = new ComplexPropertyDescription
                    {
                        PropertyName = identifier,
                        PropertyType = typeReference,
                        GetAccessModifier = new Optional<AccessModifier>(),
                        SetAccessModifier = new Optional<AccessModifier>(),
                        IsStatic = isStatic,
                        IsVirtual = isVirtual
                    };
                    if (getAccessor != null)
                    {
                        var getAccessModifier = GetAccessModifier(getAccessor.Modifiers);
                        var getModifier = MaxRestrictionAccessModifier(propertyAccessModifier, getAccessModifier);
                        complexPropertyDescription.GetAccessModifier = getModifier;
                        if (getAccessor.Body != null)
                        {
                            var statements = new List<string>();
                            foreach (var statement in getAccessor.Body.Statements)
                            {
                                string generatedStatement = StatementGenerator.Generate(statement, semanticModel);
                                statements.Add(generatedStatement);
                            }
                            complexPropertyDescription.GetStatements = statements;
                        }
                    }
                    if (setAccessor != null)
                    {
                        var setAccessModifier = GetAccessModifier(setAccessor.Modifiers);
                        var setModifier = MaxRestrictionAccessModifier(propertyAccessModifier, setAccessModifier);
                        complexPropertyDescription.SetAccessModifier = setModifier;
                        if (setAccessor.Body != null)
                        {
                            var statements = new List<string>();
                            foreach (var statement in setAccessor.Body.Statements)
                            {
                                string generatedStatement = StatementGenerator.Generate(statement, semanticModel);
                                statements.Add(generatedStatement);
                            }
                            complexPropertyDescription.SetStatements = statements;
                        }
                    }

                    SourceCode generatedProperty = PropertyGenerator.GenerateComplexProperty(complexPropertyDescription, semanticModel);
                    generatedProperties.MainPart += "\n" + generatedProperty.MainPart;
                }
                else
                {
                    var simplePropertyDescription = new SimplePropertyDescription
                    {
                        PropertyName = identifier,
                        PropertyType = typeReference,
                        GetAccessModifier = new Optional<AccessModifier>(),
                        SetAccessModifier = new Optional<AccessModifier>(),
                        IsStatic = isStatic,
                        IsVirtual = isVirtual,
                        IsFromInterface = isFromInterface
                    };
                    if (getAccessor != null)
                    {
                        var getAccessModifier = GetAccessModifier(getAccessor.Modifiers);
                        var getModifier = MaxRestrictionAccessModifier(propertyAccessModifier, getAccessModifier);
                        simplePropertyDescription.GetAccessModifier = getModifier;
                    }
                    if (setAccessor != null)
                    {
                        var setAccessModifier = GetAccessModifier(setAccessor.Modifiers);
                        var setModifier = MaxRestrictionAccessModifier(propertyAccessModifier, setAccessModifier);
                        simplePropertyDescription.SetAccessModifier = setModifier;
                    }

                    SourceCode generatedProperty = PropertyGenerator
                        .GenerateSimpleProperty(simplePropertyDescription, semanticModel, isFromInterface);
                    generatedProperties.MainPart += "\n" + generatedProperty.MainPart;
                }


            }
            return generatedProperties;
        }


        private SourceCode GetFields(SemanticModel semanticModel, HashSet<string> typeReferences,
            IEnumerable<FieldDeclarationSyntax> fieldsDeclarations)
        {
            var generatedFields = new SourceCode { MainPart = "" };
            foreach (var fieldDeclaration in fieldsDeclarations)
            {
                var variables = fieldDeclaration.Declaration.Variables;
                if (variables.Count != 1)
                {
                    throw new NotImplementedException();
                }
                var variable = variables.First();
                string identifier = variable.Identifier.ValueText;
                var fieldType = fieldDeclaration.Declaration.Type;
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(fieldType, semanticModel);
                if (!typeReference.IsPredefined)
                {
                    typeReferences.Add(typeReference.Text);
                }

                bool isStatic = HasStaticModifier(fieldDeclaration.Modifiers.ToList());
                string optionalStaticModifierText = isStatic ? "static " : "";

                var fieldAccessModifier = GetAccessModifier(fieldDeclaration.Modifiers.ToList());
                string generatedAccessModifier = fieldAccessModifier == AccessModifier.Public ? "public" : "private";

                string initializerText = "";
                if (variable.Initializer != null)
                {
                    string initializerValueText = ExpressionGenerator
                        .GenerateExpression(variable.Initializer.Value, semanticModel);
                    initializerText += " = " + initializerValueText;
                }

                string generatedField =
                    generatedAccessModifier + " " + optionalStaticModifierText + typeReference.Text + " "
                        + identifier + initializerText + ";\n";
                generatedFields.MainPart += "\n" + generatedField;
            }
            return generatedFields;
        }


        private AccessModifier GetAccessModifier(IEnumerable<SyntaxToken> modifiers)
        {
            var syntaxNodes = modifiers as IList<SyntaxToken> ?? modifiers.ToList();
            if (syntaxNodes.Any(node => node.Text == "public"))
            {
                return AccessModifier.Public;
            }
            if (syntaxNodes.Any(node => node.Text == "private"))
            {
                return AccessModifier.Private;
            }
            return AccessModifier.Public;
        }

        private bool HasStaticModifier(IEnumerable<SyntaxToken> modifiers)
        {
            return modifiers.Any(node => node.Text == "static");
        }

        private bool HasVirtualModifier(IEnumerable<SyntaxToken> modifiers)
        {
            return modifiers.Any(node => node.Text == "virtual");
        }

        private AccessModifier MaxRestrictionAccessModifier(AccessModifier accessModifier1, AccessModifier accessModifier2)
        {
            if (accessModifier1 == AccessModifier.Private || accessModifier2 == AccessModifier.Private)
            {
                return AccessModifier.Private;
            }
            return AccessModifier.Public;
        }
    }
}
