using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
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
                var binaryExpressionSyntax = (expressionSyntax as BinaryExpressionSyntax);
                string leftExpression = GenerateExpression(binaryExpressionSyntax.Left, semanticModel);
                string rightExpression = GenerateExpression(binaryExpressionSyntax.Right, semanticModel);
                string operatorString = binaryExpressionSyntax.OperatorToken.Text;

                var leftExpressionType = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, binaryExpressionSyntax.Left).Type;
                if (CustomBinaryExpressionHelper.IsCustom(leftExpressionType, operatorString))
                {
                    return CustomBinaryExpressionHelper.Generate(leftExpressionType, leftExpression, operatorString,
                        rightExpression, semanticModel);
                }
                else
                {
                    return leftExpression + " " + operatorString + " " + rightExpression;
                }
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
            var accessingType = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, elementAccessExpressionSyntax.Expression).Type;
            var arguments = elementAccessExpressionSyntax.ArgumentList.Arguments;
            if (!arguments.Any())
            {
                throw new NotImplementedException();
            }
            var argument = arguments[0];
            string generatedArgument = GenerateExpression(argument.Expression, semanticModel);
            if (CustomElementAccessHelper.IsCustom(accessingType))
            {
                return CustomElementAccessHelper.Generate(accessingType, accessingExpression, generatedArgument, elementAccessExpressionSyntax, semanticModel);
            }
            return accessingExpression + ".get(" + generatedArgument + ")";
        }

        private string GeneratePrefixUnaryExpression(PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax, SemanticModel semanticModel)
        {
            AssertOperandIsChangeble(prefixUnaryExpressionSyntax.Operand);
            return prefixUnaryExpressionSyntax.OperatorToken.Text + GenerateExpression(prefixUnaryExpressionSyntax.Operand, semanticModel);
        }

        private void AssertOperandIsChangeble(ExpressionSyntax expressionSyntax)
        {
            if (expressionSyntax is LiteralExpressionSyntax)
            {
                return;
            }
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

            var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, memberAccessExpressionSyntax);
            if (symbolInfo.Symbol.Kind == SymbolKind.Property)
            {
                string propertyName = memberAccessExpressionSyntax.Name.Identifier.ValueText;
                var type = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, memberAccessExpressionSyntax).Type;
                var ownerType = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, memberAccessExpressionSyntax.Expression).Type;
                if (CustomPropertyAccessHelper.IsCustom(ownerType, propertyName))
                {
                    return CustomPropertyAccessHelper.Generate(ownerType, memberOwnerExpression, propertyName);
                }
                else
                {
                    var typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel);
                    string methodName = PropertyGenerator.GenerateGetterMethodName(typeReference, propertyName);
                    return memberOwnerExpression + "." + GenerateMethodCallExpression(methodName, new List<string>());
                }
            }
            throw new NotImplementedException();
        }

        private string GenerateObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpressionSyntax, SemanticModel semanticModel)
        {
            var type = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, objectCreationExpressionSyntax).Type;
            if (IsExceptionType(type))
            {
                return GenerateExceptionCreationExpression(objectCreationExpressionSyntax, semanticModel);
            }
            string typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel).Text;
            string argumentList = ArgumentListGenerator.Generate(objectCreationExpressionSyntax.ArgumentList, semanticModel);
            string constructorCall = "new " + typeReference + argumentList;

            var initializer = objectCreationExpressionSyntax.Initializer;
            if (initializer == null || !initializer.Expressions.Any())
            {
                return constructorCall;
            }
            else
            {
                string objectToCreateName = "temp";

                string initializations = "";

                var expressions = objectCreationExpressionSyntax.Initializer.Expressions;
                foreach (var expressionSyntax in expressions)
                {
                    if (expressionSyntax is AssignmentExpressionSyntax)
                    {
                        var assignmentExpressionSyntax = expressionSyntax as AssignmentExpressionSyntax;
                        //string left = objectToCreateName + "." + GenerateExpression(assignmentExpressionSyntax.Left, semanticModel);
                        string expressionString = GenerateAssignmentExpression(assignmentExpressionSyntax, semanticModel);
                        initializations += objectToCreateName + "." + expressionString + ";\n";
                    }
                    else
                    {
                        //string left = objectToCreateName + "." + GenerateExpression(assignmentExpressionSyntax.Left, semanticModel);
                        string expressionString = GenerateExpression(expressionSyntax, semanticModel);
                        initializations += objectToCreateName + ".add(" + expressionString + ");\n";
                    }
                }

                string creationStatements = typeReference + " " + objectToCreateName + " = " + constructorCall + ";\n" +
                                            initializations +
                                            "return " + objectToCreateName + ";\n";

                string constructAndInitialize = "((new by.besmart.cross.delegates.Func() {\n" +
                        "\t\tpublic " + typeReference + " invoke() {\n" +
                                creationStatements.AddTab(3) +
                            "}\n" +
                        "\t}).invoke())";

                return constructAndInitialize;
            }
        }

        private string GenerateExceptionCreationExpression(ObjectCreationExpressionSyntax exceptionCreationExpressionSyntax, SemanticModel semanticModel)
        {
            var exceptionType = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, exceptionCreationExpressionSyntax).Type;
            string exceptionTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(exceptionType);
            string generatedExceptionName = "";
            if (exceptionTypeFullName == "System.ArgumentNullException")
            {
                generatedExceptionName = "IllegalArgumentException";
            }
            else if (exceptionTypeFullName == "System.ArgumentException")
            {
                generatedExceptionName = "IllegalArgumentException";
            }
            else if (exceptionTypeFullName == "System.NullReferenceException")
            {
                generatedExceptionName = "NullPointerException";
            }
            else if (exceptionTypeFullName == "System.InvalidOperationException")
            {
                generatedExceptionName = "IllegalStateException";
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
                var typeArgumentForParameter = typeSymbol.TypeArguments[i];
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(typeArgumentForParameter, semanticModel, isInGenericContext: true);
                generatedParameters.Add(new Var
                {
                    Name = parameter.Identifier.ValueText,
                    Type = typeReference
                });

            }
            var statements = new List<string>();
            if (body is BlockSyntax)
            {
                foreach (var statement in (body as BlockSyntax).Statements)
                {
                    string generatedStatement = StatementGenerator.Generate(statement, semanticModel);
                    statements.Add(generatedStatement);
                }
            }
            else if (body is InvocationExpressionSyntax)
            {
                string generatedInvocationExpression = GenerateInvocationExpression(body as InvocationExpressionSyntax,
                    semanticModel);
                string statement = generatedInvocationExpression + ";";
                if (!Equals(returnType, JavaTypeReferences.Void))
                {
                    statement = "return " + statement;
                }
                statements.Add(statement);
            }
            else if (body is LiteralExpressionSyntax)
            {
                string statement = "return " + LiteralGenerator.Generate(body as LiteralExpressionSyntax, semanticModel) + ";";
                statements.Add(statement);
            }
            else if (body is BinaryExpressionSyntax)
            {
                string statement = "return " + GenerateExpression(body as BinaryExpressionSyntax, semanticModel) + ";";
                statements.Add(statement);
            }
            return BuildLambdaExpression(delegateType, generatedParameters, returnType, statements, semanticModel);
        }

        private string BuildLambdaExpression(string delegateType, List<Var> generatedParameters, TypeReference returnType, List<string> statements, SemanticModel semanticModel)
        {
            string method = MethodGenerator.Generate("invoke", returnType, new List<string>(),
                AccessModifier.Public, generatedParameters, statements,
                false, false,
                semanticModel).MainPart;
            string generatedExpression = string.Format(@"new {0}() {{
                @Override
{1}
                }}", delegateType, method.AddTab(4));
            return generatedExpression;
        }

        private string GenerateSimpleLambdaExpression(SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax, SemanticModel semanticModel)
        {
            var parameters = new List<ParameterSyntax> { simpleLambdaExpressionSyntax.Parameter };
            var typeInfo = ModelExtensions.GetTypeInfo(semanticModel, simpleLambdaExpressionSyntax);
            var type = (INamedTypeSymbol)typeInfo.ConvertedType;
            return GenerateLambdaExpression(parameters, type, simpleLambdaExpressionSyntax.Body, semanticModel);
        }

        private string GenerateParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax, SemanticModel semanticModel)
        {
            var parameters = parenthesizedLambdaExpressionSyntax.ParameterList.Parameters.ToList();
            var typeInfo = ModelExtensions.GetTypeInfo(semanticModel, parenthesizedLambdaExpressionSyntax);
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
            var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, expression);
            if (expression is IdentifierNameSyntax)
            {
                var identifierNameExpression = expression as IdentifierNameSyntax;
                var namedTypeSymbol = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, expression).Type;
                if (namedTypeSymbol != null && TypeReferenceGenerator.IsDelegateType(namedTypeSymbol))
                {
                    return identifierNameExpression.Identifier.ValueText + "." + GenerateMethodCallExpression("invoke", parameterExpressions);
                }
                string containingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(symbolInfo.Symbol.ContainingType);
                string methodName = invocationExpressionSyntax.Expression.GetText()
                    .ToString().Trim();
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
                    string methodName = memberName;
                    if (CustomMethodInvocationHelper.IsCustom(containingTypeFullName, methodName))
                    {
                        return CustomMethodInvocationHelper.Generate(containingTypeFullName,
                            ownerExpression, methodName, parameterExpressions);
                    }
                    else
                    {
                        return ownerExpression + "." + GenerateMethodCallExpression(methodName, parameterExpressions);
                    }
                }
                if (symbolInfo.Symbol.Kind == SymbolKind.Property && symbolInfo.Symbol is IPropertySymbol)
                {
                    var propertyType = (symbolInfo.Symbol as IPropertySymbol).Type as INamedTypeSymbol;
                    if (TypeReferenceGenerator.IsDelegateType(propertyType))
                    {
                        string propertyName = memberAccessExpression.Name.Identifier.ValueText;
                        var type = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, memberAccessExpression).Type;
                        var typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel);
                        string methodName = PropertyGenerator.GenerateGetterMethodName(typeReference, propertyName);
                        return ownerExpression + "." + GenerateMethodCallExpression(methodName, new List<string>()) + "." +
                               GenerateMethodCallExpression("invoke", parameterExpressions);
                    }
                    throw new NotImplementedException();
                }
                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        private string GenerateIdentifierExpression(IdentifierNameSyntax identifierNameSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, identifierNameSyntax);
            if (symbolInfo.Symbol.Kind == SymbolKind.Property)
            {
                string propertyName = identifierNameSyntax.Identifier.ValueText;
                var type = (INamedTypeSymbol)ModelExtensions.GetTypeInfo(semanticModel, identifierNameSyntax).Type;
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(type, semanticModel);
                string methodName = PropertyGenerator.GenerateGetterMethodName(typeReference, propertyName);
                return GenerateMethodCallExpression(methodName, new List<string> { });
            }
            if (symbolInfo.Symbol.Kind == SymbolKind.Field)
            {
                return identifierNameSyntax.Identifier.ValueText;
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
                var typeSymbol = ModelExtensions.GetTypeInfo(semanticModel, identifierNameSyntax).ConvertedType as INamedTypeSymbol;
                var delegateInfo = GetDelegateInfo(typeSymbol, semanticModel);
                var delegateTypeParameters = delegateInfo.TypeParameters;

                var generatedParameters = new List<Var>();
                if (delegateInfo.IsFunc)
                {
                    delegateTypeParameters.RemoveAt(delegateTypeParameters.Count - 1);
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
                for (int i = 0; i < generatedParameters.Count - 1; i++)
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
                delegateType = TypeReferenceBuilder.BuildTypeReference(new string[] { "by.besmart.cross.delegates", delegateType },
                    typeParameters);
            }
            else
            {
                delegateType = TypeReferenceBuilder.BuildTypeReference(new string[] { "by.besmart.cross.delegates", delegateType },
                    new List<TypeReference>());
            }

            returnType = isFunc ? typeParameters.Last() : JavaTypeReferences.Void;
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

            string leftAccessingChain = "";
            if (left is MemberAccessExpressionSyntax)
            {
                var memberAccessExpressionSyntax = left as MemberAccessExpressionSyntax;
                leftAccessingChain +=
                    GenerateExpression(memberAccessExpressionSyntax.Expression, semanticModel) + ".";
                left = memberAccessExpressionSyntax.Name;
            }
            if (left is IdentifierNameSyntax)
            {
                var leftSymbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, assignmentExpressionSyntax.Left);
                if (leftSymbolInfo.Symbol.Kind == SymbolKind.Field)
                {
                    string fieldName = (left as IdentifierNameSyntax).Identifier.ValueText;
                    return leftAccessingChain + fieldName + " = " + generatedRight;
                }
                if (leftSymbolInfo.Symbol.Kind == SymbolKind.Property)
                {
                    string propertyName = (left as IdentifierNameSyntax).Identifier.ValueText;
                    string methodName = "set" + propertyName;
                    return leftAccessingChain + GenerateMethodCallExpression(methodName, new List<string> { generatedRight });
                }
                if (leftSymbolInfo.Symbol.Kind == SymbolKind.Local)
                {
                    string localName = (left as IdentifierNameSyntax).Identifier.ValueText;
                    AssertIsChangeable((left as IdentifierNameSyntax).Identifier);
                    return leftAccessingChain + localName + " = " + generatedRight;
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
            throw new NotImplementedException();
        }

        private static void AssertIsChangeable(SyntaxToken localIdentifier)
        {
            string localName = localIdentifier.ValueText;
            bool isReadOnly = !localName.StartsWith("___");
            if (isReadOnly)
            {
                throw ExceptionHelper.CreateInvalidOperationExceptionWithSourceData(
                    "You can change ONLY local variables with names starting with '___', other considered unchangeable.",
                    localIdentifier);
            }
        }

        public string GenerateMethodCallExpression(string methodName, List<string> parameterExpressions)
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
}
