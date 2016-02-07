using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MiSharp.Translator
{
    public class SyntaxTreeHelper
    {
        public static string GetFullyQualifiedName(INamedTypeSymbol typeInfo)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            string fullyQualifiedName = typeInfo.ToDisplayString(symbolDisplayFormat);
            return fullyQualifiedName;
        }

        public static string[] GetFullyQualifiedNameParts(INamedTypeSymbol typeInfo)
        {
            return GetFullyQualifiedName(typeInfo).Split('.');
        }

        public static Optional<AttributeSyntax> GetNativeImplementationAttribute(ClassDeclarationSyntax classDeclaration)
        {
            return GetAttribute(classDeclaration, Constants.NoImplementationAttributeName);
        }

        public static Optional<AttributeSyntax> GetNoImplementationAttribute(ClassDeclarationSyntax classDeclaration)
        {
            return GetAttribute(classDeclaration, Constants.NoImplementationAttributeName);
        }

        private static Optional<AttributeSyntax> GetAttribute(ClassDeclarationSyntax classDeclaration, string attributeName)
        {
            foreach (var attrs in classDeclaration.AttributeLists)
            {
                foreach (var attr in attrs.Attributes)
                {
                    if (attr.Name.ToString().StartsWith(attributeName))
                    {
                        return attr;
                    }
                }
            }
            return new Optional<AttributeSyntax>();
        }
    }
}
