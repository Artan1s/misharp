using System.Collections.Generic;
using System.IO;
using NuGet;

namespace ConsoleApplication1
{
    public class CSharpProjectSourcesCollector
    {
        public List<string> CollectProjectSources(string projectPath)
        {
            var projectDirectory = new DirectoryInfo(projectPath);
            return CollectProjectSources(projectDirectory);
        }

        private List<string> CollectProjectSources(DirectoryInfo projectDirectory)
        {
            var sources = new List<string>();
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
                string source = csFile.OpenRead().ReadToEnd();
                sources.Add(source);
            }
            return sources;
        }

        private List<string> CollectSources(DirectoryInfo sourcesDirectory)
        {
            var sources = new List<string>();
            foreach (var directory in sourcesDirectory.EnumerateDirectories())
            {
                sources.AddRange(CollectSources(directory));
            }
            foreach (var csFile in sourcesDirectory.EnumerateFiles("*.cs"))
            {
                string source = csFile.OpenRead().ReadToEnd();
                sources.Add(source);
            }
            return sources;
        }
    }
}
