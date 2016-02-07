using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaGenerator : Generator
    {
        protected override ITypeBuilder TypeBuilder
        {
            get { return new JavaTypeBuilder(); }
        }

        protected override IPredefinedTypes PredefinedTypes
        {
            get { return new JavaPredefinedTypes(); }
        }

        protected override IPropertyGenerator PropertyGenerator
        {
            get { return new JavaPropertyGenerator(); }
        }

        protected override IMethodGenerator MethodGenerator
        {
            get { return new JavaMethodGenerator(); }
        }

        protected override IConstructorGenerator ConstructorGenerator
        {
            get { return new JavaConstructorGenerator(); }
        }

        protected override IStatementGenerator StatementGenerator
        {
            get { return new JavaStatementGenerator(); }
        }

        protected override IExpressionGenerator ExpressionGenerator
        {
            get { return new JavaExpressionGenerator(); }
        }

        protected override ITypeReferenceGenerator TypeReferenceGenerator
        {
            get { return new JavaTypeReferenceGenerator(); }
        }

        protected override ITypeReferenceBuilder TypeReferenceBuilder
        {
            get { return new JavaTypeReferenceBuilder(); }
        }

        protected override IGenericTypeReferenceBuilder GenericTypeReferenceBuilder
        {
            get { return new JavaGenericTypeReferenceBuilder(); }
        }
    }
}
