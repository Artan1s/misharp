using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CP.Entities
{
    public static class A01
    {
        public static string AddTabExt(this string str, int n)
        {
            if (n > 1)
            {
                return "\t" + str.AddTabExt(n - 1);
            }
            else
            {
                return "\t" + str;
            }
        }

        public static string AddTab(string str)
        {
            return "\t" + str;
        }
    }
}
