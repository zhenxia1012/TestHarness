/////////////////////////////////////////////////////////////////////
// TestDriver3.cs - define a test to run                           //
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
    public class TestDriver3 : MarshalByRefObject, ITest
    {
        private CodeToTest3 code;  // will be compiled into separate DLL

        //----< Testdriver constructor >---------------------------------
        /*
        *  For production code the test driver may need the tested code
        *  to provide a creational function.
        */
        public TestDriver3()
        {
            code = new CodeToTest3();
        }
        //----< description of this test >-------------------------------

        public string getLog()
        {
            return "\n  Demo a failed test of using uninitialized object";
        }
        //----< test method is where all the testing gets done >---------

        public bool test()
        {
            code.annunciator();
            return true;
        }
    }

#if (TEST3)
    class Program
    {
        static void Main(string[] args)
        {
            TestDriver3 driver = new TestDriver3();
            try
            {
                driver.test();
            }
            catch (Exception ex)
            {
                Console.Write("\n  test 3 failed:{0}", ex.ToString());
            }
            Console.Write(driver.getLog());
            Console.ReadKey();
        }
    }
#endif
}
