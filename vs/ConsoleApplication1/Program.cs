using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConsoleApplication1.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Roslyn;
using NuGet;

namespace ConsoleApplication1
{
    public class Var
    {
        public string Name { get; set; }
        public TypeReference Type { get; set; }
    }
    class Program
    {
        private static SemanticModel semanticModel;

        //private static string cpDirPath = @"C:\Users\Mikhail\SkyDrive\Work\30.10.2015\ConsoleApplication2";
        private static string cpDirPath = @"C:\Users\Misha\OneDrive\Work\30.10.2015\ConsoleApplicationTest";
        private static DirectoryInfo cpDir = new DirectoryInfo(cpDirPath);

        private static DirectoryInfo cpCSharpOutputDirectory =
                new DirectoryInfo(@"C:\Users\Mikhail\Documents\visual studio 2012\Projects\PhoneApp2\CPCSharp");
        private static DirectoryInfo cpCSharpPlatformDirectory =
                new DirectoryInfo(@"C:\Users\Mikhail\Documents\visual studio 2012\Projects\PhoneApp2\CPCSharpPlatform");

        private static DirectoryInfo cpJavaOutputDirectory =
                new DirectoryInfo(@"G:\CrossPlatform\CPJavaGenerated");
        private static DirectoryInfo cpJavaPlatformDirectory =
                new DirectoryInfo(@"G:\CrossPlatform\CPJava\app\src\main\java\besmart\by\cp");


        static void Main(string[] args)
        {
//            string text =
//@"string pow4(string n) 
//{ 
//    //return m();
//    return n + "", prauet""; 
//}";
//            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
//            var root = (CompilationUnitSyntax)tree.GetRoot();
//
//            foreach (var method in root.Members.OfType<MethodDeclarationSyntax>())
//            {
//                ParseMethod(method);
//            }

            //local
            //(new JavaGenerator()).Generate(cpDirPath,
            //    @"C:\Users\Mikhail\ConsuloProjects\xcross\src");

            var crossAssemblyPath = @"C:\Users\Misha\OneDrive\Work\30.10.2015\ConsoleApplication2\bin\Debug\ConsoleApplication2.exe";
            
            var assembliesPaths = new List<string>();
            assembliesPaths.Add(crossAssemblyPath);

            (new JavaGenerator()).Generate(cpDirPath,
                @"D:\Android\j2objc\j2objctest3\shared\src\main\java",
                assembliesPaths);

            //belqi
//            (new JavaGenerator()).Generate("D:\\WindowsPhone\\Belqi\\Belqi\\PaymentSystemCore\\BuisnessEntities",
//                "D:\\Android\\payment_system_one_click_payment\\paymentsystem\\paymentsystemcore\\src\\main\\java");
            //GenerateCpCSharp();

            //GenerateCpJava();
            return;


            string clientCode =
@"using CP.Core;

namespace MyApp
{
    public class SomeService
    {
        public string GetFullName(string firstName, string secondName)
        {
            return StringUtils.Concat(firstName, secondName);
        }
    }

}";



            SyntaxTree clientCodeTree = CSharpSyntaxTree.ParseText(clientCode);
            var clientCodeRoot = (CompilationUnitSyntax)clientCodeTree.GetRoot();


            string text =
                @"namespace CP.Core
{
    [NativeImplementation]
    public static class StringUtils
    {
        public static string Concat(string str1, string str2)
        {
var a = new StringUtils();
            throw new NativeImplementationException();
        }
    }

}";




            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var comp = CSharpCompilation.Create("App").AddSyntaxTrees(clientCodeTree, tree);

            semanticModel = comp.GetSemanticModel(clientCodeTree);

            ObjCCodeGenerator.semanticModel = semanticModel;


//            var namespaceDeclaration = root.Members.OfType<NamespaceDeclarationSyntax>().First();
//
//            var classDeclaration = namespaceDeclaration.Members.OfType<ClassDeclarationSyntax>().First();
//            ObjCCodeGenerator.ProcessClass(classDeclaration, namespaceDeclaration);

            var namespaceDeclaration = clientCodeRoot.Members.OfType<NamespaceDeclarationSyntax>().First();

            var classDeclaration = namespaceDeclaration.Members.OfType<ClassDeclarationSyntax>().First();
            ObjCCodeGenerator.ProcessClass(classDeclaration, namespaceDeclaration);
        }

        private static void GenerateCpJava()
        {
            foreach (var dir in cpDir.EnumerateDirectories())
            {
                if (dir.Name.ToLower() == "bin"
                    || dir.Name.ToLower() == "obj"
                    || dir.Name.ToLower() == "properties")
                {
                    continue;
                }
                var currentDirCpJava = cpJavaOutputDirectory.CreateSubdirectory(dir.Name);
                GenerateCpJavaDir(dir, currentDirCpJava);
            }
        }

        private static void GenerateCpJavaDir(DirectoryInfo currentDirCp, DirectoryInfo currentDirCpJava)
        {
            foreach (var dir in currentDirCp.EnumerateDirectories())
            {
                var dirCpJava = cpJavaOutputDirectory.CreateSubdirectory(dir.Name);
                GenerateCpJavaDir(dir, dirCpJava);
            }
            foreach (var csFile in currentDirCp.EnumerateFiles("*.cs"))
            {
                string source = csFile.OpenRead().ReadToEnd();
                var outputSource = JavaCodeGenerator
                    .Generate(source, csFile.FullName.Remove(0, cpDir.FullName.Length),
                                    cpJavaPlatformDirectory);
                if (outputSource.HasValue)
                {
                    File.WriteAllText(currentDirCpJava.FullName + "\\"
                                      + Path.GetFileNameWithoutExtension(csFile.Name) + ".java", outputSource.Value);
                }
            }
        }

        private static void GenerateCpCSharp()
        {
            foreach (var dir in cpDir.EnumerateDirectories())
            {
                if (dir.Name.ToLower() == "bin"
                    || dir.Name.ToLower() == "obj"
                    || dir.Name.ToLower() == "properties")
                {
                    continue;
                }
                var currentDirCpCsharp = cpCSharpOutputDirectory.CreateSubdirectory(dir.Name);
                GenerateCpCSharpDir(dir, currentDirCpCsharp);
            }
        }

        private static void GenerateCpCSharpDir(DirectoryInfo currentDirCp, DirectoryInfo currentDirCpCsharp)
        {
            foreach (var dir in currentDirCp.EnumerateDirectories())
            {
                var dirCpCsharp = cpCSharpOutputDirectory.CreateSubdirectory(dir.Name);
                GenerateCpCSharpDir(dir, dirCpCsharp);
            }
            foreach (var csFile in currentDirCp.EnumerateFiles("*.cs"))
            {
                string source = csFile.OpenRead().ReadToEnd();
                var outputSource = CSharpGenerator
                    .GenerateCSharp(source, csFile.FullName.Remove(0, cpDir.FullName.Length), 
                                    cpCSharpPlatformDirectory);
                if (outputSource.HasValue)
                {
                    File.WriteAllText(currentDirCpCsharp.FullName + "\\" + csFile.Name, outputSource.Value);
                }
            }
        }


        static void ParseMethod(MethodDeclarationSyntax method)
        {
            var methodName = method.Identifier.ToString();

//            string a = string.Concat("a", "b");
//            Console.WriteLine(a);
//            return;

            Console.WriteLine("Method with name: {0}", methodName);

            var returnType = method.ReturnType.ToString();

            Console.WriteLine("Return type: {0}", returnType);

            var arguments = method.ParameterList.Parameters;

            Console.WriteLine("Args count: {0}", arguments.Count);

            foreach (var arg in arguments)
            {
                Console.WriteLine("Arg with name: {0} and type: {1}", arg.Identifier, arg.Type.GetText());
            }

            var statements = method.Body.Statements;

            Console.WriteLine("Statements count: {0}", statements.Count);

            foreach (var statement in statements)
            {
                Console.WriteLine("Statement: {0}", statement.GetText());
            }

            Console.WriteLine("---------------------------------------------\n");

//            string jCode = JavaCodeGenerator.GenerateJMethod(methodName, returnType,
//                arguments.Select(syntax =>
//                    new Var {Name = syntax.Identifier.ToString().Trim(), Type = syntax.Type.GetText().ToString().Trim()})
//                    .ToList(),
//                statements);

//            Console.WriteLine(jCode);
//
//            Console.WriteLine("---------------------------------------------\n\n");
//
//            string objCCode = ObjCCodeGenerator.GenerateObjCMethod(methodName, returnType,
//                arguments.Select(syntax =>
//                    new Var { Name = syntax.Identifier.ToString().Trim(), Type = syntax.Type.GetText().ToString().Trim() })
//                    .ToList(),
//                statements);
//
//            Console.WriteLine(objCCode);
//
//            Console.WriteLine("---------------------------------------------\n\n");
        }

        
    }

    public class ObjCCodeGenerator
    {
        public static SemanticModel semanticModel;

        static Dictionary<string, string> types = new Dictionary<string, string>
                                            {
                                                {"void", "Void"},
                                                {"int", "Int"},
                                                {"double", "Double"},
                                                {"string", "String"},
                                            };

        public static void ProcessClass(ClassDeclarationSyntax classDeclaration, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            var nativeImplementationAttribute = 
                SyntaxTreeHelper.GetNativeImplementationAttribute(classDeclaration);
            if (nativeImplementationAttribute.HasValue)
            {
                ProcessNativeImplementationClass(classDeclaration, namespaceDeclaration);
                return;
            }
            ProcessOrdinaryClass(classDeclaration, namespaceDeclaration);
        }

        private static void ProcessOrdinaryClass(ClassDeclarationSyntax classDeclaration, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            var statement1 = classDeclaration.Members.OfType<MethodDeclarationSyntax>().First().Body.Statements.First();
            var memberAccess = ((statement1 as ReturnStatementSyntax).Expression as InvocationExpressionSyntax)
                .Expression as MemberAccessExpressionSyntax;
            var identifier = (memberAccess.Expression as IdentifierNameSyntax);

            //var a = (statement1 as LocalDeclarationStatementSyntax).Declaration.Type;
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            if (typeInfo.Type.TypeKind == TypeKind.Error)
            {
                throw new Exception("unknown type " + identifier.Identifier);
            }
            
        }

        private static void ProcessNativeImplementationClass(ClassDeclarationSyntax classDeclaration, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            var namespaceString = namespaceDeclaration.Name.ToString();
            string classPrefix = namespaceString.Replace(".", "");

            var statement1 = classDeclaration.Members.OfType<MethodDeclarationSyntax>().First().Body.Statements.First();

            var a = (statement1 as LocalDeclarationStatementSyntax).Declaration.Type;
            var typeInfo = semanticModel.GetTypeInfo(a);
        }

        



        public static string GenerateObjCMethod(string name, string returnType, IEnumerable<Var> args, SyntaxList<StatementSyntax> statements)
        {
            string objCName = name;
            string objCReturnType = types[returnType];

            string objCArgs = "";
            foreach (var arg in args)
            {
                objCArgs += arg.Name + ": " + types[arg.Type.Text] + ",";
            }
            objCArgs = objCArgs.Trim(new []{' ', ','});


            string objCStatements = "";
            foreach (var statement in statements)
            {
                objCStatements += "    " + GenerateStatement(statement) + "\n";
            }


            return string.Format(
@"func {0} ({1}) -> {2} {{
{3}
}}", objCName, objCArgs, objCReturnType, objCStatements);
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
