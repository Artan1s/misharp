using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator
{
    public static class ExceptionHelper
    {
        public static InvalidOperationException CreateInvalidOperationExceptionWithSourceData(string message, SyntaxToken syntaxToken)
        {
            var exception = new InvalidOperationException(message);
            var syntaxTree = syntaxToken.SyntaxTree;
            string filePath = syntaxToken.SyntaxTree.FilePath;
            exception.Data["file"] = string.IsNullOrEmpty(filePath) ? "" : filePath;

            var span = syntaxTree.GetMappedLineSpan(syntaxToken.Span);
            exception.Data["startLineNumber"] = span.StartLinePosition.Line + 1;
            exception.Data["endLineNumber"] = span.EndLinePosition.Line + 1;
            exception.Data["startColumnNumber"] = span.StartLinePosition.Character + 1;
            exception.Data["endColumnNumber"] = span.EndLinePosition.Character + 1;

            return exception;
        }
    }
}
