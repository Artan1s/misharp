using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApplication1.CSharp
{
    public class CustomMethodInvocationHelper
    {
        private static JavaExpressionGenerator javaExpressionGenerator = new JavaExpressionGenerator();

        public static bool IsCustom(INamedTypeSymbol containingType, string methodName)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(containingType);
            return IsCustom(accessingTypeFullName, methodName);
        }

        public static bool IsCustom(string accessingTypeFullName, string methodName)
        {
            if (accessingTypeFullName == "System.Object")
            {
                return true;
            }
            else if (accessingTypeFullName == "System.String")
            {
                return true;
            }
            return false;
        }

        public static string Generate(INamedTypeSymbol containingType,
            string ownerExpression,
            string methodName,
            List<string> parameterExpressions)
        {
            string accessingTypeFullName = SyntaxTreeHelper.GetFullyQualifiedName(containingType);
            return Generate(accessingTypeFullName,
                ownerExpression,
                methodName,
                parameterExpressions);
        }

        public static string Generate(string accessingTypeFullName,
            string ownerExpression,
            string methodName,
            List<string> parameterExpressions)
        {
            if (accessingTypeFullName == "System.Object")
            {
                if (methodName == "MemberwiseClone")
                {
                    return ownerExpression + "." + 
                           javaExpressionGenerator.GenerateMethodCallExpression(
                               "clone", parameterExpressions);
                }
                else if (methodName == "GetHashCode")
                {
                    return ownerExpression + "." +
                           javaExpressionGenerator.GenerateMethodCallExpression(
                               "hashCode", parameterExpressions);
                }
            }
            else if (accessingTypeFullName == "System.String")
            {
                if (methodName == "Substring")
                {
                    parameterExpressions.Insert(0, ownerExpression);
                    return "by.besmart.cross.SystemStringUtils" + "." +
                           javaExpressionGenerator.GenerateMethodCallExpression(
                               "substring", parameterExpressions);
                }
                else if (methodName == "LastIndexOf")
                {
                    return ownerExpression + "." +
                           javaExpressionGenerator.GenerateMethodCallExpression(
                               "lastIndexOf", parameterExpressions);
                }
            }
            throw new ArgumentException();
        }
    }
}