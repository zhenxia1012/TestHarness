/////////////////////////////////////////////////////////////////////
// TestDriver5.cs - define a test to run                           //
//                                                                 //
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
    public class TestDriver5 : MarshalByRefObject, ITest
    {
        private CodeToTest5 code;  // will be compiled into separate DLL

        //----< Testdriver constructor >---------------------------------
        /*
        *  For production code the test driver may need the tested code
        *  to provide a creational function.
        */
        public TestDriver5()
        {
            code = new CodeToTest5();
        }
        //----< description of this test >-------------------------------

        public string getLog()
        {
            return "\n  demo a failed test of two integers division: 9/0";
        }
        //----< test method is where all the testing gets done >---------

        public bool test()
        {
            code.annunciator(9, 0);
            return true;
        }
    }

#if (TEST5)
    class Program
    {
        static void Main(string[] args)
        {
            TestDriver5 driver = new TestDriver5();
            try
            {
                driver.test();
            }
            catch (Exception ex)
            {
                Console.Write("\n  test 5 failed:{0}", ex.ToString());
            }
            Console.Write(driver.getLog());
            Console.ReadKey();
        }
    }
#endif
}
