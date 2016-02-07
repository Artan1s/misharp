using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator.Java
{
    public class CustomPropertyAccessHelper
    {
        private static JavaExpressionGenerator javaExpressionGenerator = new JavaExpressionGenerator();

        public static bool IsCustom(INamedTypeSymbol containingType, string propertyName)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(containingType);
            return IsCustom(accessingTypeFullName, propertyName);
        }

        public static bool IsCustom(string accessingTypeFullName, string propertyName)
        {
            if (accessingTypeFullName == "System.String")
            {
                return true;
            }
            return false;
        }

        public static string Generate(INamedTypeSymbol containingType,
            string ownerExpression,
            string propertyName)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(containingType);
            return Generate(accessingTypeFullName,
                ownerExpression,
                propertyName);
        }

        public static string Generate(string accessingTypeFullName,
            string ownerExpression,
            string propertyName)
        {
            if (accessingTypeFullName == "System.String")
            {
                if (propertyName == "Length")
                {
                    return ownerExpression + "." +
                           javaExpressionGenerator.GenerateMethodCallExpression(
                               "length", new List<string>());
                }
            }
            throw new ArgumentException();
        }
    }
}
