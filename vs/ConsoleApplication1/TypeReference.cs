using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class TypeReference
    {
        public bool IsPredefined { get; set; }

        public string Text { get; set; }

        public bool IsGeneric { get; set; }

        public bool IsReferenceType { get; set; }
    }

    public class JavaTypeReferences
    {
        public static readonly TypeReference Void = new TypeReference{Text = "void", IsPredefined = true};
    }

    public class Property
    {
        public TypeReference TypeReference { get; set; }

        public string Text { get; set; }
    }

    public class Method
    {
        public List<TypeReference> TypeReferences { get; set; }

        public string Text { get; set; }
    }
}
