using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet;

namespace ConsoleApplication1
{
    public class CSharpGenerator
    {
        private static SemanticModel semanticModel;

        public static Optional<string> GenerateCSharp(string source, string sourceCpRelativePath, DirectoryInfo cpSharpPlatformDirectory)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var comp = CSharpCompilation.Create("App").AddSyntaxTrees(tree);

            semanticModel = comp.GetSemanticModel(tree);

            var classDeclaration = (root.Members.First() as NamespaceDeclarationSyntax)
                .Members
                .First() as ClassDeclarationSyntax;

            var noImplementationAttribute =
                SyntaxTreeHelper.GetNoImplementationAttribute(classDeclaration);
            if (!noImplementationAttribute.HasValue)
            {
                return new Optional<string>();
            }

            var nativeImplementationAttribute =
                SyntaxTreeHelper.GetNativeImplementationAttribute(classDeclaration);
            if (!nativeImplementationAttribute.HasValue)
            {
                return source;
            }
            else
            {
                string nativeSource = new FileInfo(
                    cpSharpPlatformDirectory.FullName + "\\" + sourceCpRelativePath)
                    .OpenRead().ReadToEnd();
                return nativeSource;
                //ProcessNativeImplementationClass(classDeclaration, namespaceDeclaration);
            }
        }
    }
}
