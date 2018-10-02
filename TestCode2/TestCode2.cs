using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDemo
{
    public class CodeToTest2 : MarshalByRefObject
    {
        public void annunciator(int a, int b)
        {
            int tem = a / b;
            Console.Write("\n    Two integers division: {0}/{1} \n    result:{2}", a, b, tem);
        }
    }
}