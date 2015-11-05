using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApplication1.CSharp
{
    public class CustomBinaryExpressionHelper
    {
        public static bool IsCustom(INamedTypeSymbol leftOperandType, string operatorString)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(leftOperandType);
            if (accessingTypeFullName == "System.String")
            {
                if (operatorString == "==")
                {
                    return true;
                }
            }
            return false;
        }

        public static string Generate(INamedTypeSymbol leftOperandType, 
            string leftExpression,
            string operatorString, 
            string rightExpression,
            SemanticModel semanticModel)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(leftOperandType);
            if (accessingTypeFullName == "System.String")
            {
                if (operatorString == "==")
                {
                    return "by.besmart.cross.SystemStringUtils.equals(" + leftExpression + ", " + rightExpression + ")";
                }
            }
            throw new ArgumentException();
        }
    }
}