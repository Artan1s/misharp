using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MiSharp.Translator.Abstract
{
    public interface ITypeBuilder
    {
        SourceCode BuildType(SemanticModel semanticModel, string[] fullyQualifiedNameParts,
            IEnumerable<ITypeParameterSymbol> genericParameters,
            INamedTypeSymbol baseType,
            IEnumerable<INamedTypeSymbol> allInterfaces,
            SourceCode generatedConstructors,
            SourceCode fields,
            SourceCode properties,
            SourceCode methods);

        SourceCode BuildInterface(SemanticModel semanticModel, string[] fullyQualifiedNameParts,
            IEnumerable<ITypeParameterSymbol> genericParameters,
            SourceCode properties,
            SourceCode methods);

        SourceCode BuildEnum(SemanticModel semanticModel, string[] fullyQualifiedNameParts, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members);
    }
}
