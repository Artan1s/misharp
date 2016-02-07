using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaTypeReferenceGenerator : ITypeReferenceGenerator
    {
        protected ITypeReferenceBuilder TypeReferenceBuilder { get { return new JavaTypeReferenceBuilder(); } }

        protected IGenericTypeReferenceBuilder GenericTypeReferenceBuilder { get { return new JavaGenericTypeReferenceBuilder(); } }

        protected IPredefinedTypes PredefinedTypes { get { return new JavaPredefinedTypes(); } }

        public TypeReference GenerateTypeReference(ITypeSymbol typeSymbol, SemanticModel semanticModel, bool isInGenericContext = false)
        {
            if (typeSymbol is ITypeParameterSymbol)
            {
                return new TypeReference
                {
                    Text = GenericTypeReferenceBuilder.BuildReference(typeSymbol as ITypeParameterSymbol),
                    IsGeneric = true,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;
            string fullyQualifiedName = SyntaxTreeHelper.GetFullyQualifiedName(namedTypeSymbol);
            var fullyQualifiedPropertyTypeNameParts = SyntaxTreeHelper.GetFullyQualifiedNameParts(namedTypeSymbol);

            var typeParameters = new List<TypeReference>();
            if (namedTypeSymbol.IsGenericType)
            {
                foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                {
                    var typeReference = GenerateTypeReference(typeArgument, semanticModel, isInGenericContext: true);
                    typeParameters.Add(typeReference);
                }
            }

            if (PredefinedTypes.IsPredefined(fullyQualifiedName))
            {
                return new TypeReference
                {
                    Text = isInGenericContext ? PredefinedTypes.GetInGenericContext(fullyQualifiedName)
                                              : PredefinedTypes.Get(fullyQualifiedName),
                    IsPredefined = true,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            if (IsCustomImplementedType(fullyQualifiedName))
            {
                if (namedTypeSymbol.IsGenericType)
                {
                }
                string typeReferenceText = "by.misharp." + (namedTypeSymbol.IsGenericType
                    ? TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts, typeParameters)
                    : TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts));
                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            if (IsDelegateType(namedTypeSymbol))
            {
                return BuildDelegateReference(namedTypeSymbol, semanticModel);
            }

            if (namedTypeSymbol.IsGenericType)
            {
                string typeReferenceText = TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts, typeParameters);
                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
            else
            {
                string typeReferenceText = TypeReferenceBuilder.BuildTypeReference(fullyQualifiedPropertyTypeNameParts);

                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = typeSymbol.IsReferenceType
                };
            }
        }

        private bool IsCustomImplementedType(string fullyQualifiedName)
        {
            if (fullyQualifiedName == "System.Collections.Generic.List")
            {
                return true;
            }
            if (fullyQualifiedName == "System.Text.StringBuilder")
            {
                return true;
            }
            return false;
        }

        public TypeReference GenerateTypeReference(TypeSyntax typeSyntax, SemanticModel semanticModel, bool isInGenericContext = false)
        {
            var typeSymbol = semanticModel.GetTypeInfo(typeSyntax).Type;
            return GenerateTypeReference(typeSymbol, semanticModel, isInGenericContext);
        }

        public bool IsDelegateType(INamedTypeSymbol namedTypeSymbol)
        {
            string fullName = SyntaxTreeHelper.GetFullyQualifiedName(namedTypeSymbol);
            return fullName == "System.Action" || fullName == "System.Func";
        }

        public TypeReference BuildDelegateReference(INamedTypeSymbol namedTypeSymbol, SemanticModel semanticModel)
        {
            var fullyQualifiedPropertyTypeNameParts = SyntaxTreeHelper.GetFullyQualifiedNameParts(namedTypeSymbol);
            string name = namedTypeSymbol.Name;
            bool isAction = name == "Action";
            bool isFunc = name == "Func";
            if (namedTypeSymbol.IsGenericType)
            {
                var typeParameters = new List<TypeReference>();
                var args = namedTypeSymbol.TypeArguments.ToList();
                for (int i = 0; i < args.Count; i++)
                {
                    var typeArgument = args[i];
                    var typeReference = GenerateTypeReference(typeArgument, semanticModel, isInGenericContext: true);
                    typeParameters.Add(typeReference);
                    if (!(isFunc && i == 0))
                    {
                        name += "T";
                    }
                }
                string typeReferenceText = TypeReferenceBuilder
                    .BuildTypeReference(new[] { "by.besmart.cross.delegates", name }, typeParameters);
                return new TypeReference
                {
                    Text = typeReferenceText,
                    IsReferenceType = namedTypeSymbol.IsReferenceType,
                    IsGeneric = true
                };
            }
            string typeReferenceTextNonGeneric = TypeReferenceBuilder
                    .BuildTypeReference(new[] { "by.besmart.cross.delegates", name }, new List<TypeReference>());
            return new TypeReference
            {
                Text = typeReferenceTextNonGeneric,
                IsReferenceType = namedTypeSymbol.IsReferenceType
            };
        }


    }
}
