using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator.Java
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
