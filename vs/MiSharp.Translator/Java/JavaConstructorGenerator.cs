using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaConstructorGenerator : IConstructorGenerator
    {
        public SourceCode Generate(string name, AccessModifier accessModifier, IEnumerable<Var> args, List<string> statements,
            SemanticModel semanticModel)
        {
            string jAccessModifier = accessModifier == AccessModifier.Public ? "public" : "private";

            string nullGuardStatements = "";

            string jArgs = "";
            foreach (var arg in args)
            {
                jArgs += "final " + arg.Type.Text + " " + arg.Name + ",";
                // todo: handle enabling/disabling null guard
                //                if (arg.Type.IsReferenceType)
                //                {
                //                    nullGuardStatements += "\nif (" + arg.Name + " == null) {\n"
                //                                           + "    throw new IllegalArgumentException(\"" + arg.Name + "\");\n"
                //                                           + "}";
                //                }
            }
            jArgs = jArgs.Trim(new[] { ' ', ',' });


            string jStatements = nullGuardStatements;
            foreach (var statement in statements)
            {
                jStatements += "\n    " + statement;
            }


            return new SourceCode
            {
                MainPart = string.Format(
                    @"{0} {1}({2}) {{{3}
}}", jAccessModifier, name, jArgs, jStatements.AddTab()) + "\n"
            };
        }
    }
}
