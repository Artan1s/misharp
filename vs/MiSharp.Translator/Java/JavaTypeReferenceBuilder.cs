using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaTypeReferenceBuilder : ITypeReferenceBuilder
    {
        public string BuildTypeReference(string[] fullyQualifiedNameParts)
        {
            string className = fullyQualifiedNameParts.Last();
            string typeReference = "";
            for (int i = 0; i < fullyQualifiedNameParts.Length - 1; i++)
            {
                typeReference += fullyQualifiedNameParts[i].ToLower() + ".";
            }
            typeReference += className;
            return typeReference;
        }

        public string BuildTypeReference(string[] fullyQualifiedPropertyTypeNameParts, List<TypeReference> typeParameters)
        {
            string className = BuildTypeReference(fullyQualifiedPropertyTypeNameParts);
            if (typeParameters.Count == 0)
            {
                return className;
            }
            className += "<";
            for (int i = 0; i < typeParameters.Count - 1; i++)
            {
                className += typeParameters[i].Text + ", ";
            }
            className += typeParameters.Last().Text + ">";
            return className;
        }
    }
}
