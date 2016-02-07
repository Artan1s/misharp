using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator.Abstract
{
    public interface IMethodGenerator
    {
        SourceCode Generate(string name, TypeReference returnType, List<string> typeParameters,
            AccessModifier accessModifier, IEnumerable<Var> args,
            Optional<List<string>> body, bool isStatic, bool isVirtual, SemanticModel semanticModel);
    }
}
