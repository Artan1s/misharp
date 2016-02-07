using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiSharp.Translator
{
    public class CSharpProjectSourcesCollector
    {
        public List<SourceFile> CollectProjectSources(string projectPath)
        {
            var projectDirectory = new DirectoryInfo(projectPath);
            return CollectProjectSources(projectDirectory);
        }

        private List<SourceFile> CollectProjectSources(DirectoryInfo projectDirectory)
        {
            var sources = new List<SourceFile>();
            foreach (var directory in projectDirectory.EnumerateDirectories())
            {
                if (directory.Name.ToLower() == "bin"
                    || directory.Name.ToLower() == "obj"
                    || directory.Name.ToLower() == "properties")
                {
                    continue;
                }
                sources.AddRange(CollectSources(directory));
            }
            foreach (var csFile in projectDirectory.EnumerateFiles("*.cs"))
            {                
                var source = new SourceFile
                {
                    Text = File.ReadAllText(csFile.FullName),
                    Path = csFile.FullName
                };
                sources.Add(source);
            }
            return sources;
        }

        private List<SourceFile> CollectSources(DirectoryInfo sourcesDirectory)
        {
            var sources = new List<SourceFile>();
            foreach (var directory in sourcesDirectory.EnumerateDirectories())
            {
                sources.AddRange(CollectSources(directory));
            }
            foreach (var csFile in sourcesDirectory.EnumerateFiles("*.cs"))
            {
                var source = new SourceFile
                {
                    Text = File.ReadAllText(csFile.FullName),
                    Path = csFile.FullName
                };
                sources.Add(source);
            }
            return sources;
        }
    }
}
