using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaTypeReferences
    {
        public static readonly TypeReference Void = new TypeReference { Text = "void", IsPredefined = true };

        public static readonly TypeReference Bool = new TypeReference { Text = "boolean", IsPredefined = true };
    }
}
