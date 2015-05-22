using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CP.Entities
{
    public class A1Test
    {
        public void IfTest(bool condition, Action trueAction, Action falseAction)
        {
            int valI = 3;
            if (condition && valI > 3)
            {
                int k = 3;
                k = 2;
            }
            else if (valI > 10)
            {
                int k = 3;
            }
            else
            {
                falseAction();
            }
        }
    }
}