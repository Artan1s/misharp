using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator.Abstract
{
    public interface IPropertyGenerator
    {
        SourceCode GenerateSimpleProperty(SimplePropertyDescription simplePropertyDescription,
            SemanticModel semanticModel, bool isFromInterface);

        SourceCode GenerateComplexProperty(ComplexPropertyDescription complexPropertyDescription, SemanticModel semanticModel);
    }
}
