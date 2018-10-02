using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDemo
{
    public class CodeToTest1 : MarshalByRefObject
    {
        public void annunciator(string msg)
        {
            Console.Write("\n    Self-Indroduciton: {0}", msg);
        }
    }
}