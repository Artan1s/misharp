using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace MiSharp.Tools.BuildTask
{
    public class MisharpTask : Task
    {
        private readonly string configFileName = "misharp_config.xml";

        public override bool Execute()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");

            string javaOutputPath = "";

            try
            {
                var xmlConfig = new XmlDocument();
                xmlConfig.Load(ProjectDirectory + "\\" + configFileName);

                javaOutputPath = xmlConfig.GetElementsByTagName("JavaOutput").Item(0).InnerText;
            }
            catch (Exception)
            {
                Log.LogError("Invalid misharp_config");
                return false;
            }

            try
            {
                var translatorTool = new TranslatorTool();
                translatorTool.Translate(ProjectDirectory, new List<string>(), javaOutputPath);
            }
            catch (Exception exception)
            {
                string file = (string)exception.Data["file"];
                var startLineNumber = exception.Data["startLineNumber"];
                var endLineNumber = exception.Data["endLineNumber"];
                var startColumnNumber = exception.Data["startColumnNumber"];
                var endColumnNumber = exception.Data["endColumnNumber"];

                int intStartLineNumber = (int?) startLineNumber ?? 0;
                int intEndLineNumber = (int?) endLineNumber ?? 0;
                int intStartColumnNumber = (int?) startColumnNumber ?? 0;
                int intEndColumnNumber = (int?) endColumnNumber ?? 0;
                Log.LogError("",
                    "",
                    "",
                    file,
                    intStartLineNumber,
                    intStartColumnNumber,
                    intEndLineNumber,
                    intEndColumnNumber,
                    "Misharp translation error!!! " + exception.Message);
                return false;
            }
            return true;
        }

        [Required]
        public string ProjectDirectory { get; set; }

        [Required]
        public string TargetDir { get; set; }
    }
}
