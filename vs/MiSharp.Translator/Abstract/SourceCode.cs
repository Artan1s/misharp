using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiSharp.Translator.Abstract
{
    public class SourceCode
    {
        public List<string> Imports { get; set; }

        public string HeaderPart { get; set; }

        public string MainPart { get; set; }
    }
}
