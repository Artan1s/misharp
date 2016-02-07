using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MiSharp.Translator.Java
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
