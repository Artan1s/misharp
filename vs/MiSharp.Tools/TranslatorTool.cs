using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiSharp.Translator.Java;

namespace MiSharp.Tools
{
    public class TranslatorTool
    {        
        public void Translate(string sourcesPath,
            IEnumerable<string> assembliesPaths,
            string javaOutputPath)
        {
            if (string.IsNullOrEmpty(sourcesPath))
            {
                throw new ArgumentException(nameof(sourcesPath));
            }            
            if (assembliesPaths == null)
            {
                throw new ArgumentNullException(nameof(assembliesPaths));
            }

            if (string.IsNullOrEmpty(javaOutputPath))
            {
                javaOutputPath = sourcesPath + "\\java";
            }

            if (!(javaOutputPath.EndsWith("/java") || javaOutputPath.EndsWith("\\java")))
            {
                throw new ArgumentException($"Provided {nameof(javaOutputPath)} path is incorrect, it must end with 'java' directory.");
            }

            
            var javaGenerator = new JavaGenerator();
            javaGenerator.Generate(sourcesPath, javaOutputPath, assembliesPaths);
        }
    }
}
