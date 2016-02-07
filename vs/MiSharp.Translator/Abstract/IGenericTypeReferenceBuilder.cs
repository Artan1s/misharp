using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator.Abstract
{
    public interface IGenericTypeReferenceBuilder
    {
        string BuildReference(ITypeParameterSymbol typeParameterSymbol);
    }
}
