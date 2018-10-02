using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDemo
{
    public class CodeToTest5 : MarshalByRefObject
    {
        public void annunciator(int a, int b)
        {
            int tem = a / b;
            Console.Write("\n  Two integers division: {0}/{1}", a, b);
        }
    }
}