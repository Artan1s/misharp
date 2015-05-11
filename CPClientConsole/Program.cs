using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPClient;

namespace CPClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var someService = new SomeService();
            var fullName = someService.GetFullName("Mishanya", "Mladzinski");
            Console.WriteLine(fullName);
        }
    }
}
