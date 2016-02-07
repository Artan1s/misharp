using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaPredefinedTypes : IPredefinedTypes
    {
        Dictionary<string, string> jTypes = new Dictionary<string, string>
                                                   {
                                                       {"void", "void"},
                                                       {"System.Void", "void"},

                                                       {"int", "int"},
                                                       {"System.Int32", "int"},

                                                       {"double", "double"},
                                                       {"System.Double", "double"},

                                                       {"string", "String"},
                                                       {"System.String", "String"},

                                                       {"bool", "boolean"},
                                                       {"System.Boolean", "boolean"},

                                                       {"object", "Object"},
                                                       {"System.Object", "Object"},
                                                   };

        Dictionary<string, string> jTypesInGenericContext = new Dictionary<string, string>
                                                   {
                                                       {"void", "Void"},
                                                       {"System.Void", "Void"},

                                                       {"int", "Integer"},
                                                       {"System.Int32", "Integer"},

                                                       {"double", "Double"},
                                                       {"System.Double", "Double"},

                                                       {"string", "String"},
                                                       {"System.String", "String"},

                                                       {"bool", "Boolean"},
                                                       {"System.Boolean", "Boolean"},

                                                       {"object", "Object"},
                                                       {"System.Object", "Object"},
                                                   };

        public bool IsPredefined(string type)
        {
            return jTypes.ContainsKey(type);
        }

        public string Get(string type)
        {
            return jTypes[type];
        }

        public string GetInGenericContext(string type)
        {
            return jTypesInGenericContext[type];
        }
    }
}
