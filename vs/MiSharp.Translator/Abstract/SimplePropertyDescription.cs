using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MiSharp.Translator.Abstract
{
    public class SimplePropertyDescription
    {
        public string PropertyName { get; set; }

        public TypeReference PropertyType { get; set; }

        public Optional<AccessModifier> GetAccessModifier { get; set; }

        public Optional<AccessModifier> SetAccessModifier { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }

        public bool IsFromInterface { get; set; }
    }
}
