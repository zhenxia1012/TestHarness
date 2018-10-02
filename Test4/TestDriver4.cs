/////////////////////////////////////////////////////////////////////
// TestDriver4.cs - define a test to run                           //
/////////////////////////////////////////////////////////////////////            

/*
*   Test driver needs to know the types and their interfaces
*   used by the code it will test.  It doesn't need to know
*   anything about the test harness.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarness;

namespace TestDemo
{
    public class TestDriver4 : MarshalByRefObject, ITest
    {
        private CodeToTest4 code;  // will be compiled into separate DLL
        //----< Testdriver constructor >---------------------------------
        /*
        *  For production code the test driver may need the tested code
        *  to provide a creational function.
        */

        public TestDriver4()
        {
            code = new CodeToTest4();
        }
        //----< factory function >---------------------------------------

        public string getLog()
        {
            return "\n  Demo a failed test that will trigger an exception";
        }
        //----< test method is where all the testing gets done >---------

        public bool test()
        {
            code.annunciator("Hello, I am TestDriver4 in Test4");
            return true;
        }
    }

#if (Test4)
    class Program
    {
            static void Main(string[] args)
        {
            TestDriver4 driver = new TestDriver4();
            try
            {
                driver.test();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            Console.Write(driver.getLog());
            Console.ReadKey();
        }
    }
#endif
}
