using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApplication1.CSharp
{
    public interface ILiteralGenerator
    {
        string Generate(LiteralExpressionSyntax literal,
            SemanticModel semanticModel);
    }

    public class JavaLiteralGenerator : ILiteralGenerator
    {
        public string Generate(LiteralExpressionSyntax literal, SemanticModel semanticModel)
        {
            if (literal.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return "null";
            }
            if (literal.IsKind(SyntaxKind.TrueLiteralExpression))
            {
                return "true";
            }
            if (literal.IsKind(SyntaxKind.FalseLiteralExpression))
            {
                return "false";
            }
            if (literal.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return literal.ToString();
            }
            if (literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return literal.ToString();
            }
            if (literal.IsKind(SyntaxKind.CharacterLiteralExpression))
            {
                return literal.ToString();
            }
            throw new NotImplementedException();
        }
    }
}