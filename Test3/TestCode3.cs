using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDemo
{
    public class CodeToTest3 : MarshalByRefObject
    {
        public void annunciator()
        {
            System.Text.StringBuilder sb = null;
            sb.Append("won't work");
        }
    }
}