using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApplication1.CSharp
{
    public class CsGenerator : Generator
    {
        protected override ITypeBuilder TypeBuilder
        {
            get { throw new NotImplementedException(); }
        }

        protected override IPredefinedTypes PredefinedTypes
        {
            get { throw new NotImplementedException(); }
        }

        protected override IPropertyGenerator PropertyGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override IMethodGenerator MethodGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override IConstructorGenerator ConstructorGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override IStatementGenerator StatementGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override ITypeReferenceGenerator TypeReferenceGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override ITypeReferenceBuilder TypeReferenceBuilder
        {
            get { throw new NotImplementedException(); }
        }

        protected override IGenericTypeReferenceBuilder GenericTypeReferenceBuilder
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class JavaGenerator : Generator
    {
        protected override ITypeBuilder TypeBuilder
        {
            get { return new JavaTypeBuilder();}
        }

        protected override IPredefinedTypes PredefinedTypes
        {
            get { return new JavaPredefinedTypes(); }
        }

        protected override IPropertyGenerator PropertyGenerator
        {
            get { return new JavaPropertyGenerator(); }
        }

        protected override IMethodGenerator MethodGenerator
        {
            get { return new JavaMethodGenerator();}
        }

        protected override IConstructorGenerator ConstructorGenerator
        {
            get { return new JavaConstructorGenerator(); }
        }

        protected override IStatementGenerator StatementGenerator
        {
            get { return new JavaStatementGenerator(); }
        }

        protected override ITypeReferenceGenerator TypeReferenceGenerator
        {
            get { return new JavaTypeReferenceGenerator(); }
        }

        protected override ITypeReferenceBuilder TypeReferenceBuilder
        {
            get { return new JavaTypeReferenceBuilder();}
        }

        protected override IGenericTypeReferenceBuilder GenericTypeReferenceBuilder
        {
            get { return new JavaGenericTypeReferenceBuilder(); }
        }
    }

    public abstract class Generator
    {
        protected abstract ITypeBuilder TypeBuilder { get; }

        protected abstract IPredefinedTypes PredefinedTypes { get; }

        protected abstract IPropertyGenerator PropertyGenerator { get; }

        protected abstract IMethodGenerator MethodGenerator { get; }

        protected abstract IConstructorGenerator ConstructorGenerator { get; }

        protected abstract IStatementGenerator StatementGenerator { get; }

        protected abstract ITypeReferenceGenerator TypeReferenceGenerator { get; }

        protected abstract ITypeReferenceBuilder TypeReferenceBuilder { get; }

        protected abstract IGenericTypeReferenceBuilder GenericTypeReferenceBuilder { get; }

        public void Generate(string projectPath, string outputPath)
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
            Generate(sources, outputPath);
        }

        private void Generate(IEnumerable<SourceFile> sources, string outputPath)
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
                .AddReferences(mscorlib, systemCore);

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
            var enumDeclarations = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
            foreach (var enumDeclaration in enumDeclarations)
            {
                Generate(enumDeclaration, semanticModel, outputPath);
            }
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
            var generatedProperties = GetProperties(semanticModel, typeReferences, propertyDeclarations);

            var constructorDeclarations = classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            var generatedConstructors = GetConstructors(semanticModel, typeReferences, constructorDeclarations);

            var methodDeclarations = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var generatedMethods = GetMethods(semanticModel, typeReferences, methodDeclarations);

            var classSourceCode = TypeBuilder.BuildType(semanticModel, 
                fullyQualifiedNameParts, genericTypeParameters,
                typeInfo.BaseType,
                generatedConstructors,
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
                bool isStatic = methodDeclaration.Modifiers.ToList().Any(token => token.Text == "static");
                var statements = new List<string>();
                foreach (var statement in methodDeclaration.Body.Statements)
                {
                    string generatedStatement = StatementGenerator.Generate(statement, semanticModel);
                    statements.Add(generatedStatement);

                }
                var generatedMethod = MethodGenerator.Generate(identifier, returnTypeReference, accessModifier, parameters,
                    statements,
                    isStatic, 
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
            IEnumerable<PropertyDeclarationSyntax> propertyDeclarations)
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
                        SetAccessModifier = new Optional<AccessModifier>()
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
                        SetAccessModifier = new Optional<AccessModifier>()
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

                    SourceCode generatedProperty = PropertyGenerator.GenerateSimpleProperty(simplePropertyDescription, semanticModel);
                    generatedProperties.MainPart += "\n" + generatedProperty.MainPart;
                }

                
            }
            return generatedProperties;
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

        private AccessModifier MaxRestrictionAccessModifier(AccessModifier accessModifier1, AccessModifier accessModifier2)
        {
            if (accessModifier1 == AccessModifier.Private || accessModifier2 == AccessModifier.Private)
            {
                return AccessModifier.Private;
            }
            return AccessModifier.Public;
        }
    }

    public class SimplePropertyDescription
    {
        public string PropertyName { get; set; }

        public TypeReference PropertyType { get; set; }

        public Optional<AccessModifier> GetAccessModifier { get; set; }

        public Optional<AccessModifier> SetAccessModifier { get; set; }
    }

    public class ComplexPropertyDescription
    {
        public string PropertyName { get; set; }

        public TypeReference PropertyType { get; set; }

        public Optional<AccessModifier> GetAccessModifier { get; set; }

        public Optional<AccessModifier> SetAccessModifier { get; set; }

        public List<string> GetStatements { get; set; }

        public List<string> SetStatements { get; set; }
    }

    public enum AccessModifier
    {
        Public, Private
    }

    public interface ITypeBuilder
    {
        SourceCode BuildType(SemanticModel semanticModel, string[] fullyQualifiedNameParts, IEnumerable<ITypeParameterSymbol> genericParameters, 
            INamedTypeSymbol baseType, SourceCode generatedConstructors, SourceCode properties, SourceCode methods);

        SourceCode BuildEnum(SemanticModel semanticModel, string[] fullyQualifiedNameParts, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members);
    }

    public class JavaTypeBuilder : ITypeBuilder
    {
        protected JavaTypeReferenceGenerator TypeReferenceGenerator { get { return new JavaTypeReferenceGenerator(); } }

        public SourceCode BuildType(SemanticModel semanticModel, string[] fullyQualifiedNameParts, IEnumerable<ITypeParameterSymbol> genericParameters, 
            INamedTypeSymbol baseType, SourceCode constructors, SourceCode properties, SourceCode methods)
        {
            string packageName = GetPackageName(fullyQualifiedNameParts);
            string generic = "";
            var genericParametersList = genericParameters as IList<ITypeParameterSymbol> ?? genericParameters.ToList();
            if (genericParametersList.Any())
            {
                generic += "<";
                for (int i = 0; i < genericParametersList.Count() - 1; i++)
                {
                    generic += genericParametersList[i].Name + ", ";
                }
                generic += genericParametersList.Last().Name + ">";
            }
            string typeName = fullyQualifiedNameParts.Last();

            string baseTypeReferenceText = null;
            if (baseType != null)
            {
                baseTypeReferenceText = TypeReferenceGenerator.GenerateTypeReference(baseType, semanticModel).Text;
            }

            string code =
@"package " + packageName + @";

public class " + typeName + generic;
            if (!string.IsNullOrEmpty(baseTypeReferenceText))
            {
                code += " extends " + baseTypeReferenceText;
            }
            code += " {\n";
            code += constructors.MainPart.AddTab() + "\n";
            code += properties.MainPart.AddTab() + "\n";
            code += methods.MainPart.AddTab();
            code += "\n}";
            return new SourceCode
                   {
                       MainPart = code
                   };
        }

        public SourceCode BuildEnum(SemanticModel semanticModel, string[] fullyQualifiedNameParts, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            string packageName = GetPackageName(fullyQualifiedNameParts);
            string typeName = fullyQualifiedNameParts.Last();

            string membersPart = "";

            foreach (var member in members)
            {
                membersPart += member.Identifier.ValueText.ToUnderscoreCase().ToUpper() + ", ";
            }

            string code =
                @"package " + packageName + @";

public enum " + typeName;
            code += " {\n";
            code += "    " + membersPart;
            code += "\n}";
            return new SourceCode
            {
                MainPart = code
            };
        }

        private string GetPackageName(string[] fullyQualifiedNameParts)
        {
            string packageName = "";
            for (int i = 0; i < fullyQualifiedNameParts.Length - 2; i++)
            {
                packageName += fullyQualifiedNameParts[i].ToLower() + ".";
            }
            packageName += fullyQualifiedNameParts[fullyQualifiedNameParts.Length - 2].ToLower();
            return packageName;
        }
    }

    public interface IDelegateBuilder
    {
        string BuildDelegateReference(TypeInfo typeInfo, SemanticModel semanticModel);

        //string BuildDelegateInstance(TypeInfo typeInfo, SemanticModel semanticModel);
    }

//    public class JavaDelegateBuilder : IDelegateBuilder
//    {
//        
//    }

    public interface ITypeReferenceGenerator
    {
        TypeReference GenerateTypeReference(ITypeSymbol typeSymbol, SemanticModel semanticModel,
            bool isInGenericContext = false);

        TypeReference GenerateTypeReference(TypeSyntax typeSyntax, SemanticModel semanticModel,
            bool isInGenericContext = false);

        bool IsDelegateType(INamedTypeSymbol namedTypeSymbol);
    }

    public class JavaTypeReferenceGenerator : ITypeReferenceGenerator
    {
        protected ITypeReferenceBuilder TypeReferenceBuilder { get {return new JavaTypeReferenceBuilder();}}

        protected IGenericTypeReferenceBuilder GenericTypeReferenceBuilder { get { return new JavaGenericTypeReferenceBuilder(); } }

        protected IPredefinedTypes PredefinedTypes { get { return new JavaPredefinedTypes(); } }

        public TypeReference GenerateTypeReference(ITypeSymbol typeSymbol, SemanticModel semanticModel, bool isInGenericContext = false)
        {
            if (typeSymbol is ITypeParameterSymbol)
            {
                return new TypeReference
                {
                    Text = GenericTypeReferenceBuilder.BuildReference(typeSymbol as ITypeParameterSymbol),
                    IsGeneric = true,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            var namedTypeSymbol = (INamedTypeSymbol) typeSymbol;
            string fullyQualifiedName = SyntaxTreeHelper.GetFullyQualifiedName(namedTypeSymbol);
            var fullyQualifiedPropertyTypeNameParts = SyntaxTreeHelper.GetFullyQualifiedNameParts(namedTypeSymbol);

            var typeParameters = new List<TypeReference>();
            if (namedTypeSymbol.IsGenericType)
            {
                foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                {
                    var typeReference = GenerateTypeReference(typeArgument, semanticModel, isInGenericContext: true);
                    typeParameters.Add(typeReference);
                }
            }

            if (PredefinedTypes.IsPredefined(fullyQualifiedName))
            {
                return new TypeReference
                {
                    Text = isInGenericContext ? PredefinedTypes.GetInGenericContext(fullyQualifiedName)
                                              : PredefinedTypes.Get(fullyQualifiedName),
                    IsPredefined = true,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            if (IsCustomImplementedType(fullyQualifiedName))
            {
                if (namedTypeSymbol.IsGenericType)
                {
                }
                string typeReferenceText = "by.misharp." + (namedTypeSymbol.IsGenericType 
                    ? TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts, typeParameters)
                    : TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts));
                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            if (IsDelegateType(namedTypeSymbol))
            {
                return BuildDelegateReference(namedTypeSymbol, semanticModel);
            }

            if (namedTypeSymbol.IsGenericType)
            {
                string typeReferenceText = TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts, typeParameters);
                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            else
            {
                string typeReferenceText = TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts);

                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
        }

        private bool IsCustomImplementedType(string fullyQualifiedName)
        {
            if (fullyQualifiedName == "System.Collections.Generic.List")
            {
                return true;
            }
            if (fullyQualifiedName == "System.Text.StringBuilder")
            {
                return true;
            }
            return false;
        }

        public TypeReference GenerateTypeReference(TypeSyntax typeSyntax, SemanticModel semanticModel, bool isInGenericContext = false)
        {
            var typeSymbol = semanticModel.GetTypeInfo(typeSyntax).Type;
            return GenerateTypeReference(typeSymbol, semanticModel, isInGenericContext);
        }

        public bool IsDelegateType(INamedTypeSymbol namedTypeSymbol)
        {
            string fullName = SyntaxTreeHelper.GetFullyQualifiedName(namedTypeSymbol);
            return fullName == "System.Action" || fullName == "System.Func";
        }

        public TypeReference BuildDelegateReference(INamedTypeSymbol namedTypeSymbol, SemanticModel semanticModel)
        {
            var fullyQualifiedPropertyTypeNameParts = SyntaxTreeHelper.GetFullyQualifiedNameParts(namedTypeSymbol);
            string name = namedTypeSymbol.Name;
            bool isAction = name == "Action";
            bool isFunc = name == "Func";
            if (namedTypeSymbol.IsGenericType)
            {
                var typeParameters = new List<TypeReference>();
                var args = namedTypeSymbol.TypeArguments.ToList();
                for (int i = 0; i < args.Count; i++)
                {
                    var typeArgument = args[i];
                    var typeReference = GenerateTypeReference(typeArgument, semanticModel, isInGenericContext: true);
                    typeParameters.Add(typeReference);
                    if (!(isFunc && i == 0))
                    {
                        name += "T";
                    }
                }
                string typeReferenceText = TypeReferenceBuilder
                    .BuildTypeReference(new[] { "cp", name }, typeParameters);
                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = namedTypeSymbol.IsReferenceType,
                    IsGeneric = true
                };
            }
            string typeReferenceTextNonGeneric = TypeReferenceBuilder
                    .BuildTypeReference(new[] { "cp", name }, new List<TypeReference>());
            return new TypeReference
            {
                Text = typeReferenceTextNonGeneric,
                IsReferenceType = namedTypeSymbol.IsReferenceType
            };
        }

        
    }

    public interface ITypeReferenceBuilder
    {
        string BuildTypeReference(string[] fullyQualifiedNameParts);
        string BuildTypeReference(string[] fullyQualifiedPropertyTypeNameParts, List<TypeReference> typeParameters);
    }

    public class JavaTypeReferenceBuilder : ITypeReferenceBuilder
    {
        public string BuildTypeReference(string[] fullyQualifiedNameParts)
        {
            string className = fullyQualifiedNameParts.Last();
            string typeReference = "";
            for (int i = 0; i < fullyQualifiedNameParts.Length - 1; i++)
            {
                typeReference += fullyQualifiedNameParts[i].ToLower() + ".";
            }
            typeReference += className;
            return typeReference;
        }

        public string BuildTypeReference(string[] fullyQualifiedPropertyTypeNameParts, List<TypeReference> typeParameters)
        {
            string className = BuildTypeReference(fullyQualifiedPropertyTypeNameParts);
            if (typeParameters.Count == 0)
            {
                return className;
            }
            className += "<";
            for (int i = 0; i < typeParameters.Count - 1; i++)
            {
                className += typeParameters[i].Text + ", ";
            }
            className += typeParameters.Last().Text + ">";
            return className;
        }
    }

    public class JavaPropertyGenerator : IPropertyGenerator
    {
        protected IMethodGenerator MethodGenerator { get{ return new JavaMethodGenerator();} }

        protected ITypeReferenceBuilder TypeReferenceBuilder { get{return new JavaTypeReferenceBuilder();} }

        public SourceCode GenerateSimpleProperty(SimplePropertyDescription simplePropertyDescription,
            SemanticModel semanticModel)
        {
            string propertyBackingFieldName = simplePropertyDescription.PropertyName.ToLowerFirstChar();
            string backingField = 
                "private " + simplePropertyDescription.PropertyType.Text + " " + propertyBackingFieldName + ";\n";
            
            var property = new SourceCode
                           {
                               MainPart = backingField + "\n"
                           };
            if (simplePropertyDescription.GetAccessModifier.HasValue)
            {
                var getterStatements = new List<string>
                {
                    "return " + propertyBackingFieldName + ";"
                };
                string name = GenerateGetterMethodName(simplePropertyDescription.PropertyType, simplePropertyDescription.PropertyName);
                var getter = MethodGenerator.Generate(
                    name, simplePropertyDescription.PropertyType, 
                    simplePropertyDescription.GetAccessModifier.Value,
                    new List<Var>(), getterStatements,
                    false,
                    semanticModel);
                property.MainPart += getter.MainPart + "\n";
            }
            if (simplePropertyDescription.SetAccessModifier.HasValue)
            {
                var setterStatements = new List<string>
                {
                    "this." + propertyBackingFieldName + " = " + propertyBackingFieldName + ";"
                };
                var setterArgs = new List<Var>
                {
                    new Var{Name = propertyBackingFieldName, Type = simplePropertyDescription.PropertyType}
                };
                var setter = MethodGenerator.Generate(
                    "set" + simplePropertyDescription.PropertyName, JavaTypeReferences.Void,
                    simplePropertyDescription.SetAccessModifier.Value,
                    setterArgs, setterStatements,
                    false, 
                    semanticModel);
                property.MainPart += setter.MainPart + "\n";
            }
            return property;
        }

        public SourceCode GenerateComplexProperty(ComplexPropertyDescription complexPropertyDescription, SemanticModel semanticModel)
        {
            string propertyBackingFieldName = complexPropertyDescription.PropertyName.ToLowerFirstChar();

            var property = new SourceCode
            {
                MainPart = ""
            };
            if (complexPropertyDescription.GetAccessModifier.HasValue)
            {
                var getterStatements = complexPropertyDescription.GetStatements;
                string name = GenerateGetterMethodName(complexPropertyDescription.PropertyType, complexPropertyDescription.PropertyName);
                var getter = MethodGenerator.Generate(
                    name, complexPropertyDescription.PropertyType,
                    complexPropertyDescription.GetAccessModifier.Value,
                    new List<Var>(), getterStatements,
                    false,
                    semanticModel);
                property.MainPart += getter.MainPart + "\n";
            }
            if (complexPropertyDescription.SetAccessModifier.HasValue)
            {
                var setterStatements = complexPropertyDescription.SetStatements;
                var setterArgs = new List<Var>
                {
                    new Var{Name = propertyBackingFieldName, Type = complexPropertyDescription.PropertyType}
                };
                var setter = MethodGenerator.Generate(
                    "set" + complexPropertyDescription.PropertyName, JavaTypeReferences.Void,
                    complexPropertyDescription.SetAccessModifier.Value,
                    setterArgs, setterStatements,
                    false,
                    semanticModel);
                property.MainPart += setter.MainPart + "\n";
            }
            return property;
        }

        public string GenerateGetterMethodName(TypeReference propertyType, string propertyName)
        {
            if (Equals(propertyType, JavaTypeReferences.Bool))
            {
                return propertyName.ToLowerFirstChar();
            }
            else
            {
                return "get" + propertyName;
            }
        }
    }

    public interface IPropertyGenerator
    {
        SourceCode GenerateSimpleProperty(SimplePropertyDescription simplePropertyDescription,
            SemanticModel semanticModel);

        SourceCode GenerateComplexProperty(ComplexPropertyDescription complexPropertyDescription, SemanticModel semanticModel);
    }

    public class JavaMethodGenerator : IMethodGenerator
    {
        public SourceCode Generate(string name, TypeReference returnType,
            AccessModifier accessModifier,
            IEnumerable<Var> args, List<string> statements,
            bool isStatic,
            SemanticModel semanticModel)
        {
            string jname = name.ToLowerFirstChar();
            string jAccessModifier = accessModifier == AccessModifier.Public ? "public" : "private";
            if (isStatic)
            {
                jAccessModifier += " static";
            }

            string nullGuardStatements = "";

            string jArgs = "";
            foreach (var arg in args)
            {
                jArgs += "final " + arg.Type.Text + " " + arg.Name + ", ";

                // todo: handle enabling/disabling null guard
                //if (arg.Type.IsReferenceType)
                //{
                //    nullGuardStatements += "\nif (" + arg.Name + " == null) {\n"
                //                           + "    throw new IllegalArgumentException(\"" + arg.Name + "\");\n"
                //                           + "}";
                //}
            }
            jArgs = jArgs.Trim(new[] { ' ', ',' });


            string jStatements = nullGuardStatements;
            foreach (var statement in statements)
            {
                jStatements += "\n" + statement;
            }


            return new SourceCode
                   {
                       MainPart = string.Format(
                           @"{0} {1} {2}({3}) {{{4}
}}", jAccessModifier, returnType.Text, jname, jArgs, jStatements.AddTab()) + "\n"
                   };
        }
    }

    public interface IMethodGenerator
    {
        SourceCode Generate(string name, TypeReference returnType,
            AccessModifier accessModifier,
            IEnumerable<Var> args, List<string> statements,
            bool isStatic,
            SemanticModel semanticModel);
    }

    public interface IConstructorGenerator
    {
        SourceCode Generate(string name,
            AccessModifier accessModifier,
            IEnumerable<Var> args, List<string> statements,
            SemanticModel semanticModel);
    }

    public class JavaConstructorGenerator : IConstructorGenerator
    {
        public SourceCode Generate(string name, AccessModifier accessModifier, IEnumerable<Var> args, List<string> statements,
            SemanticModel semanticModel)
        {
            string jAccessModifier = accessModifier == AccessModifier.Public ? "public" : "private";

            string nullGuardStatements = "";

            string jArgs = "";
            foreach (var arg in args)
            {
                jArgs += "final " + arg.Type.Text + " " + arg.Name + ",";
                if (arg.Type.IsReferenceType)
                {
                    nullGuardStatements += "\nif (" + arg.Name + " == null) {\n"
                                           + "    throw new IllegalArgumentException(\"" + arg.Name + "\");\n"
                                           + "}";
                }
            }
            jArgs = jArgs.Trim(new[] { ' ', ',' });


            string jStatements = nullGuardStatements;
            foreach (var statement in statements)
            {
                jStatements += "\n    " + statement;
            }


            return new SourceCode
            {
                MainPart = string.Format(
                    @"{0} {1}({2}) {{{3}
}}", jAccessModifier, name, jArgs, jStatements.AddTab()) + "\n"
            };
        }
    }

    public interface IArgumentListGenerator
    {
        string Generate(ArgumentListSyntax argumentListSyntax,
            SemanticModel semanticModel);
    }

    public class JavaArgumentListGenerator : IArgumentListGenerator
    {
        public IExpressionGenerator ExpressionGenerator { get { return new JavaExpressionGenerator(); } }

        public string Generate(ArgumentListSyntax argumentListSyntax, SemanticModel semanticModel)
        {
            var parameterExpressions = new List<string>();
            foreach (var argument in argumentListSyntax.Arguments)
            {
                string parameterExpression = ExpressionGenerator.GenerateExpression(argument.Expression, semanticModel);
                parameterExpressions.Add(parameterExpression);
            }
            string argumentList = "(";
            if (parameterExpressions.Count == 0)
            {
                argumentList += ")";
                return argumentList;
            }
            for (int i = 0; i < parameterExpressions.Count - 1; i++)
            {
                argumentList += parameterExpressions[i] + ", ";
            }
            argumentList += parameterExpressions.Last() + ")";
            return argumentList;
        }
    }

    public interface IExpressionGenerator
    {
        string GenerateExpression(ExpressionSyntax expressionSyntax,
            SemanticModel semanticModel);
    }

    public class JavaExpressionGenerator : IExpressionGenerator
    {
        public ILiteralGenerator LiteralGenerator { get { return new JavaLiteralGenerator(); } }

        public IMethodGenerator MethodGenerator { get { return new JavaMethodGenerator(); } }

        public JavaPropertyGenerator PropertyGenerator { get { return new JavaPropertyGenerator(); } }

        public IArgumentListGenerator ArgumentListGenerator { get { return new JavaArgumentListGenerator(); } }

        public ITypeReferenceGenerator TypeReferenceGenerator { get { return new JavaTypeReferenceGenerator(); } }

        public ITypeReferenceBuilder TypeReferenceBuilder { get { return new JavaTypeReferenceBuilder(); } }

        public IStatementGenerator StatementGenerator { get { return new JavaStatementGenerator(); } }

        public string GenerateExpression(ExpressionSyntax expressionSyntax, SemanticModel semanticModel)
        {
            if (expressionSyntax is ObjectCreationExpressionSyntax)
            {
                return GenerateObjectCreationExpression(expressionSyntax as ObjectCreationExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is AssignmentExpressionSyntax)
            {
                return GenerateAssignmentExpression(expressionSyntax as AssignmentExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is LiteralExpressionSyntax)
            {
                return LiteralGenerator.Generate(expressionSyntax as LiteralExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is IdentifierNameSyntax)
            {
                return GenerateIdentifierExpression(expressionSyntax as IdentifierNameSyntax, semanticModel);
            }
            if (expressionSyntax is BinaryExpressionSyntax)
            {
                return GenerateExpression((expressionSyntax as BinaryExpressionSyntax).Left, semanticModel)
                       + " " + (expressionSyntax as BinaryExpressionSyntax).OperatorToken.Text
                       + " " + GenerateExpression((expressionSyntax as BinaryExpressionSyntax).Right, semanticModel);
            }
            if (expressionSyntax is PostfixUnaryExpressionSyntax)
            {
                return GeneratePostfixUnaryExpression(expressionSyntax as PostfixUnaryExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is PrefixUnaryExpressionSyntax)
            {
                return GeneratePrefixUnaryExpression(expressionSyntax as PrefixUnaryExpressionSyntax, semanticModel);
                
            }
            if (expressionSyntax is ParenthesizedExpressionSyntax)
            {
                return "("
                       + GenerateExpression((expressionSyntax as ParenthesizedExpressionSyntax).Expression, semanticModel)
                       + ")";
            }
            if (expressionSyntax is MemberAccessExpressionSyntax)
            {
                return GenerateMemberAccessExpression(expressionSyntax as MemberAccessExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is ElementAccessExpressionSyntax)
            {
                return GenerateElementAccessExpression(expressionSyntax as ElementAccessExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is InvocationExpressionSyntax)
            {
                return GenerateInvocationExpression(expressionSyntax as InvocationExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is ParenthesizedLambdaExpressionSyntax)
            {
                return GenerateParenthesizedLambdaExpression(expressionSyntax as ParenthesizedLambdaExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is SimpleLambdaExpressionSyntax)
            {
                return GenerateSimpleLambdaExpression(expressionSyntax as SimpleLambdaExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is CastExpressionSyntax)
            {
                return GenerateCastExpression(expressionSyntax as CastExpressionSyntax, semanticModel);
            }
            if (expressionSyntax is ThisExpressionSyntax)
            {
                return GenerateThisExpression(expressionSyntax as ThisExpressionSyntax, semanticModel);
            }
            throw new NotImplementedException();
        }

        private string GenerateThisExpression(ThisExpressionSyntax thisExpressionSyntax, SemanticModel semanticModel)
        {
            return "this";
        }

        private string GenerateCastExpression(CastExpressionSyntax castExpressionSyntax, SemanticModel semanticModel)
        {
            var typeReference = TypeReferenceGenerator.GenerateTypeReference(castExpressionSyntax.Type, semanticModel);
            return "(" + typeReference.Text + ") " + GenerateExpression(castExpressionSyntax.Expression, semanticModel);
        }

        private string GenerateElementAccessExpression(ElementAccessExpressionSyntax elementAccessExpressionSyntax, SemanticModel semanticModel)
        {
            string accessingExpression = GenerateExpression(elementAccessExpressionSyntax.Expression, semanticModel);
            var arguments = elementAccessExpressionSyntax.ArgumentList.Arguments;
            if (!arguments.Any())
            {
                throw new NotImplementedException();
            }
            var argument = arguments[0];
            string generatedArgument = GenerateExpression(argument.Expression, semanticModel);
            return accessingExpression + ".get(" + generatedArgument + ")";
        }

        private string GeneratePrefixUnaryExpression(PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax, SemanticModel semanticModel)
        {
            AssertOperandIsChangeble(prefixUnaryExpressionSyntax.Operand);
            return prefixUnaryExpressionSyntax.OperatorToken.Text + GenerateExpression(prefixUnaryExpressionSyntax.Operand, semanticModel);
        }

        private void AssertOperandIsChangeble(ExpressionSyntax expressionSyntax)
        {
            var identifierNameSyntax = expressionSyntax as IdentifierNameSyntax;
            if (identifierNameSyntax == null)
            {
                throw new InvalidOperationException();
            }
            AssertIsChangeable(identifierNameSyntax.Identifier);
        }

        private string GeneratePostfixUnaryExpression(PostfixUnaryExpressionSyntax postfixUnaryExpressionSyntax, SemanticModel semanticModel)
        {
            AssertOperandIsChangeble(postfixUnaryExpressionSyntax.Operand);
            return GenerateExpression(postfixUnaryExpressionSyntax.Operand, semanticModel)
                + postfixUnaryExpressionSyntax.OperatorToken.Text;
        }

        private string GenerateMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpressionSyntax, SemanticModel semanticModel)
        {
            string memberOwnerExpression = GenerateExpression(memberAccessExpressionSyntax.Expression, semanticModel);
            
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax);
            if (symbolInfo.Symbol.Kind == SymbolKind.Property)
            {
                string propertyName = memberAccessExpressionSyntax.Name.Identifier.ValueText;
                var type = (INamedTypeSymbol)semanticModel.GetTypeInfo(memberAccessExpressionSyntax).Type;
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel);
                string methodName = PropertyGenerator.GenerateGetterMethodName(typeReference, propertyName); ;
                return memberOwnerExpression + "." + GenerateMethodCallExpression(methodName, new List<string>());
            }
            throw new NotImplementedException();
        }

        private string GenerateObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpressionSyntax, SemanticModel semanticModel)
        {
            var type = (INamedTypeSymbol) semanticModel.GetTypeInfo(objectCreationExpressionSyntax).Type;
            if (IsExceptionType(type))
            {
                return GenerateExceptionCreationExpression(objectCreationExpressionSyntax, semanticModel);
            }
            string typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel).Text;
            string argumentList = ArgumentListGenerator.Generate(objectCreationExpressionSyntax.ArgumentList, semanticModel);
            return "new " + typeReference + argumentList;
        }

        private string GenerateExceptionCreationExpression(ObjectCreationExpressionSyntax exceptionCreationExpressionSyntax, SemanticModel semanticModel)
        {
            var exceptionType = (INamedTypeSymbol)semanticModel.GetTypeInfo(exceptionCreationExpressionSyntax).Type;
            string exceptionTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(exceptionType);
            string generatedExceptionName = "";
            if (exceptionTypeFullName == "System.ArgumentNullException")
            {
                generatedExceptionName = "IllegalArgumentException";
            }
            else
            {
                throw new NotSupportedException();
            }
            string argumentList = ArgumentListGenerator.Generate(exceptionCreationExpressionSyntax.ArgumentList, semanticModel);
            return "new " + generatedExceptionName + argumentList;
        }

        private bool IsExceptionType(INamedTypeSymbol typeSymbol)
        {
            string baseExceptionType = "System.Exception";
            while (typeSymbol != null)
            {
                string typeFullName = SyntaxTreeHelper.GetFullyQualifiedName(typeSymbol);
                if (typeFullName == baseExceptionType)
                {
                    return true;
                }
                typeSymbol = typeSymbol.BaseType;
            }
            return false;
        }

        private string GenerateLambdaExpression(List<ParameterSyntax> parameters, INamedTypeSymbol typeSymbol, CSharpSyntaxNode body, SemanticModel semanticModel)
        {
            var delegateInfo = GetDelegateInfo(typeSymbol, semanticModel);
            string delegateType = delegateInfo.JavaDelegateType;
            TypeReference returnType = delegateInfo.ReturnType;

            bool isFunc = delegateInfo.IsFunc;

            var generatedParameters = new List<Var>();
            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var typeArgumentForParameter = isFunc ? typeSymbol.TypeArguments[i + 1] : typeSymbol.TypeArguments[i];
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(typeArgumentForParameter, semanticModel, isInGenericContext: true);
                generatedParameters.Add(new Var
                {
                    Name = parameter.Identifier.ValueText,
                    Type = typeReference
                });

            }
            var statements = new List<string>();
            if (body is InvocationExpressionSyntax)
            {
                string generatedInvocationExpression = GenerateInvocationExpression(body as InvocationExpressionSyntax,
                    semanticModel);
                string statement = generatedInvocationExpression + ";";
                if (!Equals(returnType, JavaTypeReferences.Void))
                {
                    statement += "return ";
                }
                statements.Add(statement);
            }
            else if (body is LiteralExpressionSyntax)
            {
                string statement = "return " + LiteralGenerator.Generate(body as LiteralExpressionSyntax, semanticModel) + ";";
                statements.Add(statement);
            }
            else if (body is BlockSyntax)
            {
                foreach (var statement in (body as BlockSyntax).Statements)
                {
                    string generatedStatement = StatementGenerator.Generate(statement, semanticModel);
                    statements.Add(generatedStatement);
                }
            }
            return BuildLambdaExpression(delegateType, generatedParameters, returnType, statements, semanticModel);
        }

        private string BuildLambdaExpression(string delegateType, List<Var> generatedParameters, TypeReference returnType, List<string> statements, SemanticModel semanticModel)
        {
            string method = MethodGenerator.Generate("invoke", returnType, AccessModifier.Public, generatedParameters, statements,
                false,
                semanticModel).MainPart;
            string generatedExpression = string.Format(@"new {0}() {{
                @Override
{1}
                }}", delegateType, method.AddTab(4));
            return generatedExpression;
        }

        private string GenerateSimpleLambdaExpression(SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax, SemanticModel semanticModel)
        {
            var parameters = new List<ParameterSyntax> {simpleLambdaExpressionSyntax.Parameter};
            var typeInfo = semanticModel.GetTypeInfo(simpleLambdaExpressionSyntax);
            var type = (INamedTypeSymbol)typeInfo.ConvertedType;
            return GenerateLambdaExpression(parameters, type, simpleLambdaExpressionSyntax.Body, semanticModel);   
        }

        private string GenerateParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax, SemanticModel semanticModel)
        {
            var parameters = parenthesizedLambdaExpressionSyntax.ParameterList.Parameters.ToList();
            var typeInfo = semanticModel.GetTypeInfo(parenthesizedLambdaExpressionSyntax);
            var type = (INamedTypeSymbol)typeInfo.ConvertedType;
            return GenerateLambdaExpression(parameters, type, parenthesizedLambdaExpressionSyntax.Body, semanticModel);            
        }

        private string GenerateInvocationExpression(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            var parameterExpressions = new List<string>();
            foreach (var argument in invocationExpressionSyntax.ArgumentList.Arguments)
            {
                string parameterExpression = GenerateExpression(argument.Expression, semanticModel);
                parameterExpressions.Add(parameterExpression);
            }
            var expression = invocationExpressionSyntax.Expression;
            var symbolInfo = semanticModel.GetSymbolInfo(expression);
            if (expression is IdentifierNameSyntax)
            {
                var identifierNameExpression = expression as IdentifierNameSyntax;
                var namedTypeSymbol = (INamedTypeSymbol)semanticModel.GetTypeInfo(expression).Type;
                if (namedTypeSymbol != null && TypeReferenceGenerator.IsDelegateType(namedTypeSymbol))
                {
                    return identifierNameExpression.Identifier.ValueText + "." + GenerateMethodCallExpression("invoke", parameterExpressions);
                }
                string containingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(symbolInfo.Symbol.ContainingType);
                string methodName = OverwriteMethodNameIfNeeded(invocationExpressionSyntax.Expression.GetText().ToString().Trim(), containingTypeFullName); 
                return GenerateMethodCallExpression(methodName, parameterExpressions);
            }
            if (expression is MemberAccessExpressionSyntax)
            {
                var memberAccessExpression = expression as MemberAccessExpressionSyntax;
                string memberName = memberAccessExpression.Name.Identifier.ValueText;
                
                string ownerExpression = GenerateExpression(memberAccessExpression.Expression, semanticModel);
                if (symbolInfo.Symbol is IMethodSymbol && (symbolInfo.Symbol as IMethodSymbol).IsExtensionMethod)
                {
                    var containingType = (symbolInfo.Symbol as IMethodSymbol).ContainingType;
                    string containingTypeReference = TypeReferenceGenerator.GenerateTypeReference(containingType, semanticModel).Text;
                    parameterExpressions.Insert(0, ownerExpression);
                    return containingTypeReference + "." + GenerateMethodCallExpression(memberName, parameterExpressions);
                }
                if (symbolInfo.Symbol.Kind == SymbolKind.Method)
                {                    
                    string containingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(symbolInfo.Symbol.ContainingType);
                    string methodName = OverwriteMethodNameIfNeeded(memberName, containingTypeFullName);
                    return ownerExpression + "." + GenerateMethodCallExpression(methodName, parameterExpressions);
                }
                if (symbolInfo.Symbol.Kind == SymbolKind.Property && symbolInfo.Symbol is IPropertySymbol)
                {
                    var propertyType = (symbolInfo.Symbol as IPropertySymbol).Type as INamedTypeSymbol;
                    if (TypeReferenceGenerator.IsDelegateType(propertyType))
                    {
                        string propertyName = memberAccessExpression.Name.Identifier.ValueText;
                        var type = (INamedTypeSymbol)semanticModel.GetTypeInfo(memberAccessExpression).Type;
                        var typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel);
                        string methodName = PropertyGenerator.GenerateGetterMethodName(typeReference, propertyName);
                        return ownerExpression + "." + GenerateMethodCallExpression(methodName, new List<string>()) +"." +
                               GenerateMethodCallExpression("invoke", parameterExpressions);
                    }
                    throw new NotImplementedException();
                }
                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        private string OverwriteMethodNameIfNeeded(string methodName, string containingTypeFullName)
        {
            if (containingTypeFullName == "System.Object")
            {
                if (methodName == "MemberwiseClone")
                {
                    return "clone";
                }
                if (methodName == "GetHashCode")
                {
                    return "hashCode";
                }
            }
            return methodName;
        }

        private string GenerateIdentifierExpression(IdentifierNameSyntax identifierNameSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifierNameSyntax);
            if (symbolInfo.Symbol.Kind == SymbolKind.Property)
            {
                string propertyName = identifierNameSyntax.Identifier.ValueText;
                var type = (INamedTypeSymbol)semanticModel.GetTypeInfo(identifierNameSyntax).Type;
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel);
                string methodName = PropertyGenerator.GenerateGetterMethodName(typeReference, propertyName);
                return GenerateMethodCallExpression(methodName, new List<string> { });
            }
            if (symbolInfo.Symbol.Kind == SymbolKind.Parameter)
            {
                return identifierNameSyntax.Identifier.ValueText;
            }
            if (symbolInfo.Symbol.Kind == SymbolKind.Local)
            {
                return identifierNameSyntax.Identifier.ValueText;
            }
            if (symbolInfo.Symbol.Kind == SymbolKind.NamedType)
            {
                return TypeReferenceGenerator.GenerateTypeReference((ITypeSymbol)symbolInfo.Symbol, semanticModel).Text;
            }
            if (symbolInfo.Symbol.Kind == SymbolKind.Method)
            {
                var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
                var typeSymbol = semanticModel.GetTypeInfo(identifierNameSyntax).ConvertedType as INamedTypeSymbol;
                var delegateInfo = GetDelegateInfo(typeSymbol, semanticModel);
                var delegateTypeParameters = delegateInfo.TypeParameters;

                var generatedParameters = new List<Var>();
                if (delegateInfo.IsFunc)
                {
                    delegateTypeParameters.RemoveAt(0);
                }
                for (int i = 0; i < delegateTypeParameters.Count; i++)
                {
                    generatedParameters.Add(new Var
                    {
                        Name = "arg" + i,
                        Type = delegateTypeParameters[i]
                    });
                }
                string statement = delegateInfo.IsFunc ? "return " : "";
                statement += methodSymbol.Name.ToLowerFirstChar() + "(";
                for (int i = 0; i < generatedParameters.Count-1; i++)
                {
                    statement += generatedParameters[i].Name + ", ";
                }
                if (generatedParameters.Count > 0)
                {
                    statement += generatedParameters[generatedParameters.Count - 1].Name;
                }
                statement += ");";
                var statements = new List<string>
                {
                    statement
                };
                return BuildLambdaExpression(delegateInfo.JavaDelegateType, generatedParameters, delegateInfo.ReturnType, statements, semanticModel);
            }
            throw new NotImplementedException();
        }

        private DelegateInfo GetDelegateInfo(INamedTypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            string delegateType = "";
            TypeReference returnType = null;

            bool isFunc = typeSymbol.Name == "Func";
            if (isFunc)
            {
                delegateType = "Func";
            }
            else
            {
                delegateType = "Action";
            }

            var typeParameters = new List<TypeReference>();
            if (typeSymbol.IsGenericType)
            {
                for (int i = 0; i < typeSymbol.TypeArguments.Count(); i++)
                {
                    var typeArgument = typeSymbol.TypeArguments[i];
                    var typeReference = TypeReferenceGenerator.GenerateTypeReference(typeArgument, semanticModel, isInGenericContext: true);
                    typeParameters.Add(typeReference);
                    if (!(isFunc && i == 0))
                    {
                        delegateType += "T";
                    }
                }
                delegateType = TypeReferenceBuilder.BuildTypeReference(new string[] { "cp", delegateType },
                    typeParameters);
            }
            else
            {
                delegateType = TypeReferenceBuilder.BuildTypeReference(new string[] { "cp", delegateType },
                    new List<TypeReference>());
            }

            returnType = isFunc ? typeParameters[0] : JavaTypeReferences.Void;
            return new DelegateInfo
            {
                ReturnType = returnType,
                JavaDelegateType = delegateType,
                TypeParameters = typeParameters,
                IsFunc = isFunc
            };
        }

        private string GenerateAssignmentExpression(AssignmentExpressionSyntax assignmentExpressionSyntax, SemanticModel semanticModel)
        {
            string generatedRight = GenerateExpression(assignmentExpressionSyntax.Right, semanticModel);
            var left = assignmentExpressionSyntax.Left;
            if (left is IdentifierNameSyntax)
            {
                var leftSymbolInfo = semanticModel.GetSymbolInfo(assignmentExpressionSyntax.Left);
                if (leftSymbolInfo.Symbol.Kind == SymbolKind.Property)
                {
                    string propertyName = (left as IdentifierNameSyntax).Identifier.ValueText;
                    string methodName = "set" + propertyName;
                    return GenerateMethodCallExpression(methodName, new List<string> { generatedRight });
                }
                if (leftSymbolInfo.Symbol.Kind == SymbolKind.Local)
                {
                    string localName = (left as IdentifierNameSyntax).Identifier.ValueText;
                    AssertIsChangeable((left as IdentifierNameSyntax).Identifier);
                    return localName + "=" + generatedRight;
                }
                if (leftSymbolInfo.Symbol.Kind == SymbolKind.Parameter)
                {
                    throw new InvalidOperationException("Don't try to change parameters. All parameters considered unchangeable.");
                }
            }
            if (left is ElementAccessExpressionSyntax)
            {
                var elementAccessExpressionSyntax = left as ElementAccessExpressionSyntax;
                string accessingExpression = GenerateExpression(elementAccessExpressionSyntax.Expression, semanticModel);
                var arguments = elementAccessExpressionSyntax.ArgumentList.Arguments;
                if (!arguments.Any())
                {
                    throw new NotImplementedException();
                }
                var argument = arguments[0];
                string generatedArgument = GenerateExpression(argument.Expression, semanticModel);
                return accessingExpression + ".set(" + generatedArgument + ", " + generatedRight + ")";
            }
            if (left is MemberAccessExpressionSyntax)
            {
                var memberAccessExpressionSyntax = left as MemberAccessExpressionSyntax;
                return "this." + memberAccessExpressionSyntax.Name + " = " + generatedRight;
            }
            //return "";
            throw new NotImplementedException();
        }

        private static void AssertIsChangeable(SyntaxToken localIdentifier)
        {
            string localName = localIdentifier.ValueText;
            bool isReadOnly = !localName.StartsWith("val");
            if (isReadOnly)
            {
                throw ExceptionHelper.CreateInvalidOperationExceptionWithSourceData(
                    "You can change ONLY local variables with names starting with 'val', other considered unchangeable.",
                    localIdentifier);
            }
        }

        private string GenerateMethodCallExpression(string methodName, List<string> parameterExpressions)
        {
            string jName = methodName.ToLowerFirstChar();
            string methodCall = jName + "(";
            if (parameterExpressions.Count == 0)
            {
                methodCall += ")";
                return methodCall;
            }
            for (int i = 0; i < parameterExpressions.Count - 1; i++)
            {
                methodCall += parameterExpressions[i] + ", ";
            }
            methodCall += parameterExpressions.Last() + ")";
            return methodCall;
        }
    }

    public interface IStatementGenerator
    {
        ILiteralGenerator LiteralGenerator { get; }

        string Generate(StatementSyntax statements,
            SemanticModel semanticModel);
    }

    public class JavaStatementGenerator : IStatementGenerator
    {
        public ILiteralGenerator LiteralGenerator { get { return new JavaLiteralGenerator(); } }

        public IMethodGenerator MethodGenerator { get { return new JavaMethodGenerator(); } }

        public ITypeReferenceGenerator TypeReferenceGenerator { get { return new JavaTypeReferenceGenerator(); } }

        public ITypeReferenceBuilder TypeReferenceBuilder { get { return new JavaTypeReferenceBuilder(); } }

        public IExpressionGenerator ExpressionGenerator { get { return new JavaExpressionGenerator(); } }

        public string Generate(StatementSyntax statement, SemanticModel semanticModel)
        {
            if (statement is IfStatementSyntax)
            {
                return GenerateIfStatement((statement as IfStatementSyntax), semanticModel);
            }
            if (statement is ForEachStatementSyntax)
            {
                return GenerateForEachStatement((statement as ForEachStatementSyntax), semanticModel);
            }
            if (statement is LocalDeclarationStatementSyntax)
            {
                return GenerateLocalDeclaration((statement as LocalDeclarationStatementSyntax), semanticModel) + ";";
            }
            if (statement is ExpressionStatementSyntax)
            {
                return ExpressionGenerator.GenerateExpression((statement as ExpressionStatementSyntax).Expression, semanticModel) + ";";
            }
            if (statement is ReturnStatementSyntax)
            {
                return "return " + ExpressionGenerator.GenerateExpression((statement as ReturnStatementSyntax).Expression, semanticModel) + ";";
            }
            if (statement is ThrowStatementSyntax)
            {
                return "throw " + ExpressionGenerator.GenerateExpression((statement as ThrowStatementSyntax).Expression, semanticModel) + ";";
            }
            throw new NotImplementedException();
        }

        private string GenerateForEachStatement(ForEachStatementSyntax forEachStatementSyntax, SemanticModel semanticModel)
        {
            string variable = forEachStatementSyntax.Identifier.ValueText;
            var symbolInfo = semanticModel.GetSymbolInfo(forEachStatementSyntax.Type);
            string variableTypeText;
            if (symbolInfo.Symbol is INamedTypeSymbol)
            {
                var namedTypeSymbol = symbolInfo.Symbol as INamedTypeSymbol;
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(namedTypeSymbol, semanticModel);
                variableTypeText = typeReference.Text;
            }
            else if (symbolInfo.Symbol is ITypeParameterSymbol)
            {
                variableTypeText = (symbolInfo.Symbol as ITypeParameterSymbol).Name;
            }
            else
            {
                throw new NotImplementedException();
            }
            string iterable = ExpressionGenerator.GenerateExpression(forEachStatementSyntax.Expression, semanticModel);
            string jStatements = GenerateBlock(forEachStatementSyntax.Statement as BlockSyntax, semanticModel);
            return "for (" + variableTypeText + " " + variable + " : " + iterable + ") {\n"
                + "    " + jStatements + "\n"
                + "}";
        }

        private string GenerateIfStatement(IfStatementSyntax ifStatementSyntax, SemanticModel semanticModel)
        {
            string condition = ExpressionGenerator.GenerateExpression(ifStatementSyntax.Condition, semanticModel);
            if (!(ifStatementSyntax.Statement is BlockSyntax))
            {
                throw new NotImplementedException();
            }

            string jStatements = GenerateBlock(ifStatementSyntax.Statement as BlockSyntax, semanticModel);

            string ifStatement = "if (" + condition + ") {";
            if (!string.IsNullOrEmpty(jStatements))
            {
                ifStatement += "\n";
            }
            ifStatement += jStatements.AddTab();

            ifStatement += "\n}";
            if (ifStatementSyntax.Else == null)
            {
                return ifStatement;
            }


            ifStatement += " else ";
            if (ifStatementSyntax.Else.Statement is IfStatementSyntax)
            {
                ifStatement += GenerateIfStatement(ifStatementSyntax.Else.Statement as IfStatementSyntax, semanticModel);
                return ifStatement;
            }
            if (!(ifStatementSyntax.Else.Statement is BlockSyntax))
            {
                throw new NotImplementedException();
            }
            ifStatement += "{";
            string elseStatements = GenerateBlock(ifStatementSyntax.Else.Statement as BlockSyntax, semanticModel);
            if (!string.IsNullOrEmpty(elseStatements))
            {
                ifStatement += "\n";
            }
            ifStatement += elseStatements.AddTab() + "\n}";
            return ifStatement;
        }

        private string GenerateBlock(BlockSyntax blockSyntax, SemanticModel semanticModel)
        {
            var statements = new List<string>();
            foreach (var statement in blockSyntax.Statements)
            {
                string generatedStatement = Generate(statement, semanticModel);
                statements.Add(generatedStatement);
            }
            string jStatements = "";
            foreach (var statement in statements)
            {
                jStatements += "\n" + statement;
            }
            jStatements = jStatements.Trim('\n');
            return jStatements;
        }

        private string GenerateLocalDeclaration(LocalDeclarationStatementSyntax localDeclarationStatementSyntax, SemanticModel semanticModel)
        {
            var declaration = localDeclarationStatementSyntax.Declaration;
            var declarationType = declaration.Type;
            string generatedDeclarationType = TypeReferenceGenerator.GenerateTypeReference(declarationType, semanticModel).Text;
            if (declaration.Variables.Count > 1)
            {
                throw new NotImplementedException();
            }
            var declarationVariable = declaration.Variables[0];
            string variableName = declarationVariable.Identifier.ValueText;
            
            string valueExpression = ExpressionGenerator.GenerateExpression(declarationVariable.Initializer.Value, semanticModel);
            string generatedDeclaration =  generatedDeclarationType + " " + variableName + " = " + valueExpression;

            if (!variableName.StartsWith("val"))
            {
                generatedDeclaration = "final " + generatedDeclaration;
            }
            return generatedDeclaration;
        }

        
    }

    public class JavaPredefinedTypes : IPredefinedTypes
    {
        Dictionary<string, string> jTypes = new Dictionary<string, string>
                                                   {
                                                       {"void", "void"},
                                                       {"System.Void", "void"},

                                                       {"int", "int"},
                                                       {"System.Int32", "int"},

                                                       {"double", "double"},
                                                       {"System.Double", "double"},

                                                       {"string", "String"},
                                                       {"System.String", "String"},

                                                       {"bool", "boolean"},
                                                       {"System.Boolean", "boolean"},

                                                       {"object", "Object"},
                                                       {"System.Object", "Object"},
                                                   };

        Dictionary<string, string> jTypesInGenericContext = new Dictionary<string, string>
                                                   {
                                                       {"void", "Void"},
                                                       {"System.Void", "Void"},

                                                       {"int", "Integer"},
                                                       {"System.Int32", "Integer"},

                                                       {"double", "Double"},
                                                       {"System.Double", "Double"},

                                                       {"string", "String"},
                                                       {"System.String", "String"},

                                                       {"bool", "Boolean"},
                                                       {"System.Boolean", "Boolean"},

                                                       {"object", "Object"},
                                                       {"System.Object", "Object"},
                                                   };

        public bool IsPredefined(string type)
        {
            return jTypes.ContainsKey(type);
        }

        public string Get(string type)
        {
            return jTypes[type];
        }

        public string GetInGenericContext(string type)
        {
            return jTypesInGenericContext[type];
        }
    }

    public interface IPredefinedTypes
    {
        bool IsPredefined(string type);

        string Get(string type);

        string GetInGenericContext(string type);
    }

    public class SourceCode
    {
        public List<string> Imports { get; set; }

        public string HeaderPart { get; set; }

        public string MainPart { get; set; }
    }
}
