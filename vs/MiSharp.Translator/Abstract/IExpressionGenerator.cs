using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MiSharp.Translator.Abstract
{
    public interface IExpressionGenerator
    {
        string GenerateExpression(ExpressionSyntax expressionSyntax,
            SemanticModel semanticModel);
    }
}
