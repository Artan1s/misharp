using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaMethodGenerator : IMethodGenerator
    {
        public SourceCode Generate(string name, TypeReference returnType, List<string> typeParameters,
            AccessModifier accessModifier, IEnumerable<Var> args,
            Optional<List<string>> body, bool isStatic, bool isVirtual, SemanticModel semanticModel)
        {
            string jname = name.ToLowerFirstChar();
            string jAccessModifier = accessModifier == AccessModifier.Public ? "public" : "private";
            if (isStatic)
            {
                jAccessModifier += " static";
            }
            if (!isVirtual)
            {
                jAccessModifier += " final";
            }

            var jTypeParametersSb = new StringBuilder();
            if (typeParameters.Any())
            {
                jTypeParametersSb.Append("<");
                for (int i = 0; i < typeParameters.Count - 1; i++)
                {
                    jTypeParametersSb.Append(typeParameters[i] + ",");
                }
                jTypeParametersSb.Append(typeParameters.Last() + ">");
            }


            string nullGuardStatements = "";

            string jArgs = "";
            foreach (var arg in args)
            {
                jArgs += "final " + arg.Type.Text + " " + arg.Name + ", ";

                // todo: handle enabling/disabling null guard
                //if (arg.Type.IsReferenceType)
                //{
                //    nullGuardStatements += "\nif (" + arg.Name + " == null) {\n"
                //                           + "    throw new IllegalArgumentException(\"" + arg.Name + "\");\n"
                //                           + "}";
                //}
            }
            jArgs = jArgs.Trim(new[] { ' ', ',' });


            var jBodyStringBuilder = new StringBuilder();
            if (body.HasValue)
            {
                jBodyStringBuilder.Append("{");
                jBodyStringBuilder.Append(nullGuardStatements);
                foreach (var statement in body.Value)
                {
                    jBodyStringBuilder.Append("\n\t");
                    jBodyStringBuilder.Append(statement);
                }
                jBodyStringBuilder.Append("\n}");
            }
            else
            {
                jBodyStringBuilder.Append(";");
            }


            return new SourceCode
            {
                MainPart = string.Format(
                    @"{0} {1} {2} {3}({4}) {5}", jAccessModifier, jTypeParametersSb, returnType.Text,
 jname, jArgs, jBodyStringBuilder) + "\n"
            };
        }
    }
}
