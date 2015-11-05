using System;
using System.Collections.Generic;
using ConsoleApplication1.CSharp;

namespace ConsoleApplication1.ObjectiveC
{
    public class ObjectiveCGenerator : Generator
    {
        protected override ITypeBuilder TypeBuilder
        {
            get { throw new NotImplementedException(); }
        }

        protected override IPredefinedTypes PredefinedTypes
        {
            get { throw new NotImplementedException(); }
        }

        protected override IPropertyGenerator PropertyGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override IMethodGenerator MethodGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override IConstructorGenerator ConstructorGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override IStatementGenerator StatementGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override ITypeReferenceGenerator TypeReferenceGenerator
        {
            get { throw new NotImplementedException(); }
        }

        protected override ITypeReferenceBuilder TypeReferenceBuilder
        {
            get { throw new NotImplementedException(); }
        }

        protected override IGenericTypeReferenceBuilder GenericTypeReferenceBuilder
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class ObjectiveCPredefinedTypes : IPredefinedTypes
    {
        Dictionary<string, string> types = new Dictionary<string, string>
                                                   {
                                                       {"void", "void"},
                                                       {"System.Void", "void"},

                                                       {"int", "int"},
                                                       {"System.Int32", "int"},

                                                       {"double", "double"},
                                                       {"System.Double", "double"},

                                                       {"string", "NSString"},
                                                       {"System.String", "NSString"},

                                                       {"bool", "BOOL"},
                                                       {"System.Boolean", "BOOL"},

                                                       {"object", "NSObject"},
                                                       {"System.Object", "NSObject"},
                                                   };

        Dictionary<string, string> typesInGenericContext = new Dictionary<string, string>
                                                   {
                                                       {"void", "Void"},
                                                       {"System.Void", "Void"},

                                                       {"int", "Integer"},
                                                       {"System.Int32", "Integer"},

                                                       {"double", "Double"},
                                                       {"System.Double", "Double"},

                                                       {"string", "NSString"},
                                                       {"System.String", "NSString"},

                                                       {"bool", "Boolean"},
                                                       {"System.Boolean", "Boolean"},

                                                       {"object", "NSObject"},
                                                       {"System.Object", "NSObject"},
                                                   };

        public bool IsPredefined(string type)
        {
            return types.ContainsKey(type);
        }

        public string Get(string type)
        {
            return types[type];
        }

        public string GetInGenericContext(string type)
        {
            return typesInGenericContext[type];
        }
    }
}
