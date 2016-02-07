using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiSharp.Translator.Abstract
{
    public interface IPredefinedTypes
    {
        bool IsPredefined(string type);

        string Get(string type);

        string GetInGenericContext(string type);
    }
}
