using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace MiSharp.Tools.Console
{
    public class Options
    {
        [Option('s', "source", Required = false, 
          HelpText = "Path to source files files to be translated. If not provided - working directory will be used.")]
        public string SourcesPath { get; set; }

        [Option('a', "assemblies", Required = false,
                  HelpText = "Additional assemblies' paths required to compile source files.")]
        public IEnumerable<string> AssembliesFiles { get; set; }

        [Option('j', "java", Required = false, 
          HelpText = "Path to java output folder. Need to end with '\\java'. If not provided - source\\java will be used.")]
        public string JavaOutputPath { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("Usage: misharp -s sourcesPath");
            return usage.ToString();
        }
    }
}
