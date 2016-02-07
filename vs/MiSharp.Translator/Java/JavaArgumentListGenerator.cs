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
    public class JavaArgumentListGenerator : IArgumentListGenerator
    {
        public IExpressionGenerator ExpressionGenerator { get { return new JavaExpressionGenerator(); } }

        public string Generate(ArgumentListSyntax argumentListSyntax, SemanticModel semanticModel)
        {
            var parameterExpressions = new List<string>();
            if (argumentListSyntax != null)
            {
                foreach (var argument in argumentListSyntax.Arguments)
                {
                    string parameterExpression = ExpressionGenerator.GenerateExpression(argument.Expression, semanticModel);
                    parameterExpressions.Add(parameterExpression);
                }
            }
            string argumentList = "(";
            if (parameterExpressions.Count == 0)
            {
                argumentList += ")";
                return argumentList;
            }
            for (int i = 0; i < parameterExpressions.Count - 1; i++)
            {
                argumentList += parameterExpressions[i] + ", ";
            }
            argumentList += parameterExpressions.Last() + ")";
            return argumentList;
        }
    }
}
