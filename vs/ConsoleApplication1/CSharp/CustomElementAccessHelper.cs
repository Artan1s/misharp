using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApplication1.CSharp
{
    public class CustomElementAccessHelper
    {
        public static bool IsCustom(INamedTypeSymbol accessingType)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(accessingType);
            if (accessingTypeFullName == "System.String")
            {
                return true;
            }
            return false;
        }

        public static string Generate(INamedTypeSymbol accessingType, 
            string accessingExpression, 
            string generatedArgument, 
            ElementAccessExpressionSyntax elementAccessExpressionSyntax, 
            SemanticModel semanticModel)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(accessingType);
            if (accessingTypeFullName == "System.String")
            {
                return accessingExpression + ".charAt(" + generatedArgument + ")";
            }
            throw new ArgumentException();
        }
    }
}