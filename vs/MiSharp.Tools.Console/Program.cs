using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiSharp.Tools.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var succeed = CommandLine.Parser.Default.ParseArguments(args, options);
            if (succeed)
            {
                if (string.IsNullOrEmpty(options.SourcesPath))
                {
                    options.SourcesPath = Environment.CurrentDirectory;
                }
                if (options.AssembliesFiles == null)
                {
                    options.AssembliesFiles = new List<string>();
                }
                var translator = new TranslatorTool();
                translator.Translate(options.SourcesPath, options.AssembliesFiles, options.JavaOutputPath);
            }
            else
            {
                System.Console.WriteLine(options.GetUsage());
            }          
        }
    }
}
