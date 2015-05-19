using System.Collections.Generic;

namespace ConsoleApplication1.CSharp
{
    public class DelegateInfo
    {
        public TypeReference ReturnType { get; set; }
        public string JavaDelegateType { get; set; }
        public List<TypeReference> TypeParameters { get; set; }
        public bool IsFunc { get; set; }
    }
}