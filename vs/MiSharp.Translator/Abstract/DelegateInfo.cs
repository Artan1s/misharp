using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MiSharp.Translator.Abstract
{
    public class DelegateInfo
    {
        public TypeReference ReturnType { get; set; }
        public string JavaDelegateType { get; set; }
        public List<TypeReference> TypeParameters { get; set; }
        public bool IsFunc { get; set; }
    }
}
