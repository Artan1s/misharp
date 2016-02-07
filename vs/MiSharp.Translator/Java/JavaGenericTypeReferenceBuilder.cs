using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaGenericTypeReferenceBuilder : IGenericTypeReferenceBuilder
    {
        public string BuildReference(ITypeParameterSymbol typeParameterSymbol)
        {
            string genericTypeReference = typeParameterSymbol.Name;
            return genericTypeReference;
        }
    }
}
