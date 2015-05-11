using Microsoft.CodeAnalysis;

namespace ConsoleApplication1.CSharp
{
    public interface IGenericTypeReferenceBuilder
    {
        string BuildReference(ITypeParameterSymbol typeParameterSymbol);
    }

    public class JavaGenericTypeReferenceBuilder : IGenericTypeReferenceBuilder
    {
        public string BuildReference(ITypeParameterSymbol typeParameterSymbol)
        {
            string genericTypeReference = typeParameterSymbol.Name;
            return genericTypeReference;
        }
    }
}