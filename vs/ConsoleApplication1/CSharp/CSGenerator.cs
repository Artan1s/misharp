using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApplication1.CSharp
{

    

    

    

    

    

    

    

    public interface IDelegateBuilder
    {
        string BuildDelegateReference(TypeInfo typeInfo, SemanticModel semanticModel);

        //string BuildDelegateInstance(TypeInfo typeInfo, SemanticModel semanticModel);
    }

//    public class JavaDelegateBuilder : IDelegateBuilder
//    {
//        
//    }

    

    

    

    

    

    

    

    

    

    

    

    

    

    

    

    

    

    

    
}
