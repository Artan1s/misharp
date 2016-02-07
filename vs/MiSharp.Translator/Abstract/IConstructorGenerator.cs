using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator.Abstract
{
    public interface IConstructorGenerator
    {
        SourceCode Generate(string name,
            AccessModifier accessModifier,
            IEnumerable<Var> args, List<string> statements,
            SemanticModel semanticModel);
    }
}
