using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MiSharp.Translator.Abstract
{
    public interface ITypeReferenceGenerator
    {
        TypeReference GenerateTypeReference(ITypeSymbol typeSymbol, SemanticModel semanticModel,
            bool isInGenericContext = false);

        TypeReference GenerateTypeReference(TypeSyntax typeSyntax, SemanticModel semanticModel,
            bool isInGenericContext = false);

        bool IsDelegateType(INamedTypeSymbol namedTypeSymbol);
    }
}
