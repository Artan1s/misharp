﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ConsoleApplication1.CSharp;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Task = Microsoft.Build.Utilities.Task;

namespace ClassLibrary1
{
    public class WorkingTask : Task
    {
        private readonly string configFileName = "misharp_config.xml";

        public override bool Execute()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");
            string path = "D:\\11111";
            //Directory.CreateDirectory(path);

            string javaOutputPath = "";

            try
            {
                var xmlConfig = new XmlDocument();
                xmlConfig.Load(ProjectDirectory + "\\" + configFileName);
                var javaOutputElements = xmlConfig.GetElementsByTagName("JavaOutput");
                
                javaOutputPath = xmlConfig.GetElementsByTagName("JavaOutput").Item(0).InnerText;
            }
            catch (Exception)
            {
                Log.LogError("Invalid misharp_config");
                return false;
            }


            if (string.IsNullOrEmpty(javaOutputPath))
            {
                javaOutputPath = TargetDir + "\\generated_java_sources";
            }

            try
            {
                (new JavaGenerator()).Generate(ProjectDirectory, javaOutputPath);
            }
            catch (Exception exception)
            {
                string file = (string) exception.Data["file"];
                var startLineNumber = exception.Data["startLineNumber"];
                var endLineNumber = exception.Data["endLineNumber"];
                var startColumnNumber = exception.Data["startColumnNumber"];
                var endColumnNumber = exception.Data["endColumnNumber"];

                int intStartLineNumber = startLineNumber == null ? 0 : (int)startLineNumber;
                int intEndLineNumber = endLineNumber == null ? 0 : (int)endLineNumber;
                int intStartColumnNumber = startColumnNumber == null ? 0 : (int)startColumnNumber;
                int intEndColumnNumber = endColumnNumber == null ? 0 : (int)endColumnNumber;
                Log.LogError("",
                    "",
                    "",
                    file,
                    intStartLineNumber,
                    intStartColumnNumber,
                    intEndLineNumber,
                    intEndColumnNumber,
                    "Compilation error!!! " + exception.Message);
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
