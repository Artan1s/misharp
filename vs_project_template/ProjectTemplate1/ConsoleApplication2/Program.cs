using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary1;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            (new WorkingTask{ProjectDirectory = @"C:\Users\Mikhail\Documents\visual studio 2012\Projects\ProjectTemplate1\ConsoleApplication1"}).Execute();
        }
    }
}
