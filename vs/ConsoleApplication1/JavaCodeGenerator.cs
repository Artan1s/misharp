using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet;

namespace ConsoleApplication1
{
    public class JavaCodeGenerator
    {
        public string JavaPlatformDirectory { get; set; }

        static Dictionary<string, string> jTypes = new Dictionary<string, string>
                                                   {
                                                       {"void", "int"},
                                                       {"int", "int"},
                                                       {"double", "double"},
                                                       {"string", "String"},
                                                   };


        public static Optional<string> Generate(string source, string sourceCpRelativePath, DirectoryInfo cpJavaPlatformDirectory)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            //var comp = CSharpCompilation.Create("App").AddSyntaxTrees(tree);

            //semanticModel = comp.GetSemanticModel(tree);

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
                    cpJavaPlatformDirectory.FullName + "\\" 
                        + FormJavaRelativePath(sourceCpRelativePath))
                    .OpenRead().ReadToEnd();
                return nativeSource;
                //ProcessNativeImplementationClass(classDeclaration, namespaceDeclaration);
            }
        }

        private static string FormJavaRelativePath(string sourceCpRelativePath)
        {
            var pathParts = sourceCpRelativePath.Split('\\');
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                pathParts[i] = pathParts[i].ToLower();
            }
            pathParts[pathParts.Length - 1] =
                Path.GetFileNameWithoutExtension(pathParts[pathParts.Length - 1]) + ".java";
            return string.Join("\\", pathParts);
        }


        public static void ProcessClass(ClassDeclarationSyntax classDeclaration, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            var nativeImplementationAttribute = GetNativeImplementationAttribute(classDeclaration);
            if (nativeImplementationAttribute.HasValue)
            {
                ProcessNativeImplementationClass(classDeclaration, namespaceDeclaration);
            }
            else
            {
                ProcessOrdinaryClass(classDeclaration, namespaceDeclaration);
            }
        }

        private static void ProcessOrdinaryClass(ClassDeclarationSyntax classDeclaration, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            
        }

        private static void ProcessNativeImplementationClass(ClassDeclarationSyntax classDeclaration, NamespaceDeclarationSyntax namespaceDeclaration)
        {

        }

        private static Optional<AttributeSyntax> GetNativeImplementationAttribute(ClassDeclarationSyntax classDeclaration)
        {
            foreach (var attrs in classDeclaration.AttributeLists)
            {
                foreach (var attr in attrs.Attributes)
                {
                    if (attr.Name.ToString().StartsWith(Constants.NativeImplementationAttributeName))
                    {
                        return attr;
                    }
                }
            }
            return new Optional<AttributeSyntax>();
        }



        public static string GenerateJMethod(string name, string returnType, IEnumerable<Var> args, SyntaxList<StatementSyntax> statements)
        {
            string jname = name.ToLowerFirstChar();
            string jReturnType = jTypes[returnType];

            string jArgs = "";
            foreach (var arg in args)
            {
                jArgs += jTypes[arg.Type.Text] + " " + arg.Name + ",";
            }
            jArgs = jArgs.Trim(new[] { ' ', ',' });


            string jStatements = "";
            foreach (var statement in statements)
            {
                jStatements += "    " + GenerateStatement(statement) + "\n";
            }


            return string.Format(
                @"{0} {1}({2}) {{
{3}
}}", jReturnType, jname, jArgs, jStatements);
        }

        public static string GenerateStatement(StatementSyntax statement)
        {
            if (statement is ReturnStatementSyntax)
            {
                var expression = (statement as ReturnStatementSyntax).Expression;
                return "return " + GenerateExpression(expression) + ";";
            }
            throw new NotSupportedException();
        }

        public static string GenerateExpression(ExpressionSyntax expression)
        {
            if (expression is BinaryExpressionSyntax)
            {
                return GenerateExpression((expression as BinaryExpressionSyntax).Left)
                       + (expression as BinaryExpressionSyntax).OperatorToken.Text
                       + GenerateExpression((expression as BinaryExpressionSyntax).Right);
            }
            if (expression is ParenthesizedExpressionSyntax)
            {
                return "("
                       + GenerateExpression((expression as ParenthesizedExpressionSyntax).Expression)
                       + ")";
            }
            if (expression is LiteralExpressionSyntax)
            {
                return expression.ToString();
            }
            if (expression is IdentifierNameSyntax)
            {
                return (expression as IdentifierNameSyntax).Identifier.ToString();
            }
            throw new NotSupportedException();
        }
    }
}