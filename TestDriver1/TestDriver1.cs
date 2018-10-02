/////////////////////////////////////////////////////////////////////
// TestDriver1.cs - define a test to run                           //
/////////////////////////////////////////////////////////////////////
/*
*   Test driver needs to know the types and their interfaces
*   used by the code it will test.  It doesn't need to know
*   anything about the test harness.
*/
//#define Test1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarness;

namespace TestDemo
{
    public class TestDriver1 : MarshalByRefObject, ITest
    {
        private CodeToTest1 code;  // will be compiled into separate DLL

        //----< Testdriver constructor >---------------------------------
        /*
        *  For production code the test driver may need the tested code
        *  to provide a creational function.
        */
        public TestDriver1()
        {
            code = new CodeToTest1();
        }
        //----< description of this test >-------------------------------

        public string getLog()
        {
            return "Demo a simply successful test of display a string";
        }

        //----< test method is where all the testing gets done >---------

        public bool test()
        {
            code.annunciator("Hello, I am TestDriver1 in Test1.");
            return true;
        }
    }

#if (Test1)
    class Program
    {
        static void Main(string[] args)
        {
            TestDriver1 driver = new TestDriver1();
            driver.test();
            Console.Write(driver.getLog());
            Console.Read();

        }
    }
#endif

}
