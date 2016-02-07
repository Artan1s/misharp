using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiSharp.Translator.Abstract
{
    public interface ITypeReferenceBuilder
    {
        string BuildTypeReference(string[] fullyQualifiedNameParts);
        string BuildTypeReference(string[] fullyQualifiedPropertyTypeNameParts, List<TypeReference> typeParameters);
    }
}
