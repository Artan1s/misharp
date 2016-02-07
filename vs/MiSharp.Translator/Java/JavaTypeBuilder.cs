using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaTypeBuilder : ITypeBuilder
    {
        protected JavaTypeReferenceGenerator TypeReferenceGenerator { get { return new JavaTypeReferenceGenerator(); } }

        public SourceCode BuildType(SemanticModel semanticModel, string[] fullyQualifiedNameParts,
            IEnumerable<ITypeParameterSymbol> genericParameters,
            INamedTypeSymbol baseType,
            IEnumerable<INamedTypeSymbol> allInterfaces,
            SourceCode constructors,
            SourceCode fields,
            SourceCode properties,
            SourceCode methods)
        {
            string packageName = GetPackageName(fullyQualifiedNameParts);
            string generic = "";
            var genericParametersList = genericParameters as IList<ITypeParameterSymbol> ?? genericParameters.ToList();
            if (genericParametersList.Any())
            {
                generic += "<";
                for (int i = 0; i < genericParametersList.Count() - 1; i++)
                {
                    generic += genericParametersList[i].Name + ", ";
                }
                generic += genericParametersList.Last().Name + ">";
            }
            string typeName = fullyQualifiedNameParts.Last();

            string baseTypeReferenceText = null;
            if (baseType != null)
            {
                baseTypeReferenceText = TypeReferenceGenerator.GenerateTypeReference(baseType, semanticModel).Text;
            }

            string interfacesDeclaration = "";

            if (allInterfaces.Any())
            {
                var interfaceReferenceTexts = new List<string>();
                foreach (var interfaceNamedTypeSymbol in allInterfaces)
                {
                    interfaceReferenceTexts.Add(
                        TypeReferenceGenerator.GenerateTypeReference(interfaceNamedTypeSymbol, semanticModel).Text);
                }
                interfacesDeclaration = " implements " + String.Join(", ", interfaceReferenceTexts.ToArray());
            }

            string code =
@"package " + packageName + @";

public class " + typeName + generic;
            if (!string.IsNullOrEmpty(baseTypeReferenceText))
            {
                code += " extends " + baseTypeReferenceText;
            }
            code += interfacesDeclaration;
            code += " {\n";
            code += fields.MainPart.AddTab() + "\n";
            code += constructors.MainPart.AddTab() + "\n";
            code += properties.MainPart.AddTab() + "\n";
            code += methods.MainPart.AddTab();
            code += "\n}";
            return new SourceCode
            {
                MainPart = code
            };
        }

        public SourceCode BuildInterface(SemanticModel semanticModel, string[] fullyQualifiedNameParts,
            IEnumerable<ITypeParameterSymbol> genericParameters,
            SourceCode properties,
            SourceCode methods)
        {
            string packageName = GetPackageName(fullyQualifiedNameParts);
            string generic = "";
            var genericParametersList = genericParameters as IList<ITypeParameterSymbol> ?? genericParameters.ToList();
            if (genericParametersList.Any())
            {
                generic += "<";
                for (int i = 0; i < genericParametersList.Count() - 1; i++)
                {
                    generic += genericParametersList[i].Name + ", ";
                }
                generic += genericParametersList.Last().Name + ">";
            }
            string typeName = fullyQualifiedNameParts.Last();



            string code =
@"package " + packageName + @";

public interface " + typeName + generic;

            code += " {\n";
            code += properties.MainPart.AddTab() + "\n";
            code += methods.MainPart.AddTab();
            code += "\n}";
            return new SourceCode
            {
                MainPart = code
            };
        }

        public SourceCode BuildEnum(SemanticModel semanticModel, string[] fullyQualifiedNameParts, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            string packageName = GetPackageName(fullyQualifiedNameParts);
            string typeName = fullyQualifiedNameParts.Last();

            string membersPart = "";

            foreach (var member in members)
            {
                membersPart += member.Identifier.ValueText.ToUnderscoreCase().ToUpper() + ", ";
            }

            string code =
                @"package " + packageName + @";

public enum " + typeName;
            code += " {\n";
            code += "    " + membersPart;
            code += "\n}";
            return new SourceCode
            {
                MainPart = code
            };
        }

        private string GetPackageName(string[] fullyQualifiedNameParts)
        {
            string packageName = "";
            for (int i = 0; i < fullyQualifiedNameParts.Length - 2; i++)
            {
                packageName += fullyQualifiedNameParts[i].ToLower() + ".";
            }
            packageName += fullyQualifiedNameParts[fullyQualifiedNameParts.Length - 2].ToLower();
            return packageName;
        }
    }
}
