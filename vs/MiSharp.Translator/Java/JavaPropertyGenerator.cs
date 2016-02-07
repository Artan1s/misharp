using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaPropertyGenerator : IPropertyGenerator
    {
        protected IMethodGenerator MethodGenerator { get { return new JavaMethodGenerator(); } }

        protected ITypeReferenceBuilder TypeReferenceBuilder { get { return new JavaTypeReferenceBuilder(); } }

        public SourceCode GenerateSimpleProperty(SimplePropertyDescription simplePropertyDescription,
            SemanticModel semanticModel, bool isFromInterface)
        {
            string propertyBackingFieldName = simplePropertyDescription.PropertyName.ToLowerFirstChar();
            string optionalStaticModifier = simplePropertyDescription.IsStatic ? "static " : "";
            string backingField =
                "private " + optionalStaticModifier
                + simplePropertyDescription.PropertyType.Text + " " + propertyBackingFieldName + ";\n";

            SourceCode property;
            if (isFromInterface)
            {
                property = new SourceCode
                {
                    MainPart = ""
                };
            }
            else
            {
                property = new SourceCode
                {
                    MainPart = backingField + "\n"
                };
            }

            if (simplePropertyDescription.GetAccessModifier.HasValue)
            {
                Optional<List<string>> getterStatements;
                if (isFromInterface)
                {
                    getterStatements = new Optional<List<string>>();
                }
                else
                {
                    getterStatements = new List<string>
                    {
                        "return " + propertyBackingFieldName + ";"
                    };
                }
                string name = GenerateGetterMethodName(simplePropertyDescription.PropertyType, simplePropertyDescription.PropertyName);
                var getter = MethodGenerator.Generate(
                    name, simplePropertyDescription.PropertyType,
                    new List<string>(),
                    simplePropertyDescription.GetAccessModifier.Value,
                    new List<Var>(), getterStatements,
                    simplePropertyDescription.IsStatic,
                    simplePropertyDescription.IsVirtual,
                    semanticModel);
                property.MainPart += getter.MainPart + "\n";
            }
            if (simplePropertyDescription.SetAccessModifier.HasValue)
            {
                Optional<List<string>> setterStatements;
                if (isFromInterface)
                {
                    setterStatements = new Optional<List<string>>();
                }
                else
                {
                    string fieldAccessor = simplePropertyDescription.IsStatic ? "" : "this.";
                    setterStatements = new List<string>
                    {
                        fieldAccessor + propertyBackingFieldName + " = " + "value" + ";"
                    };
                }


                var setterArgs = new List<Var>
                {
                    new Var{Name = "value", Type = simplePropertyDescription.PropertyType}
                };
                var setter = MethodGenerator.Generate(
                    "set" + simplePropertyDescription.PropertyName, JavaTypeReferences.Void,
                    new List<string>(),
                    simplePropertyDescription.SetAccessModifier.Value,
                    setterArgs, setterStatements,
                    simplePropertyDescription.IsStatic,
                    simplePropertyDescription.IsVirtual,
                    semanticModel);
                property.MainPart += setter.MainPart + "\n";
            }
            return property;
        }

        public SourceCode GenerateComplexProperty(ComplexPropertyDescription complexPropertyDescription, SemanticModel semanticModel)
        {
            string propertyBackingFieldName = complexPropertyDescription.PropertyName.ToLowerFirstChar();
            //string optionalStaticModifier = complexPropertyDescription.IsStatic ? "static " : "";
            //string backingField =
            //    "private " + optionalStaticModifier
            //    + complexPropertyDescription.PropertyType.Text + " " + propertyBackingFieldName + ";\n";

            var property = new SourceCode
            {
                MainPart = ""
            };
            if (complexPropertyDescription.GetAccessModifier.HasValue)
            {
                var getterStatements = complexPropertyDescription.GetStatements;
                string name = GenerateGetterMethodName(complexPropertyDescription.PropertyType, complexPropertyDescription.PropertyName);
                var getter = MethodGenerator.Generate(
                    name, complexPropertyDescription.PropertyType,
                    new List<string>(),
                    complexPropertyDescription.GetAccessModifier.Value,
                    new List<Var>(), getterStatements,
                    complexPropertyDescription.IsStatic,
                    complexPropertyDescription.IsVirtual,
                    semanticModel);
                property.MainPart += getter.MainPart + "\n";
            }
            if (complexPropertyDescription.SetAccessModifier.HasValue)
            {
                var setterStatements = complexPropertyDescription.SetStatements;
                var setterArgs = new List<Var>
                {
                    new Var{Name = "value", Type = complexPropertyDescription.PropertyType}
                };
                var setter = MethodGenerator.Generate(
                    "set" + complexPropertyDescription.PropertyName, JavaTypeReferences.Void,
                    new List<string>(),
                    complexPropertyDescription.SetAccessModifier.Value,
                    setterArgs, setterStatements,
                    complexPropertyDescription.IsStatic,
                    complexPropertyDescription.IsVirtual,
                    semanticModel);
                property.MainPart += setter.MainPart + "\n";
            }
            return property;
        }

        public string GenerateGetterMethodName(TypeReference propertyType, string propertyName)
        {
            if (Equals(propertyType, JavaTypeReferences.Bool))
            {
                return propertyName.ToLowerFirstChar();
            }
            else
            {
                return "get" + propertyName;
            }
        }
    }
}
