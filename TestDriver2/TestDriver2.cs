/////////////////////////////////////////////////////////////////////
// TestDriver2.cs - define a test to run                           //
//                                                                 //
/*
*   Test driver needs to know the types and their interfaces
*   used by the code it will test.  It doesn't need to know
*   anything about the test harness.
*/
#define TEST2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarness;

namespace TestDemo
{
    public class TestDriver2 : MarshalByRefObject, ITest
    {
        private CodeToTest2 code;  // will be compiled into separate DLL

        //----< Testdriver constructor >---------------------------------
        /*
        *  For production code the test driver may need the tested code
        *  to provide a creational function.
        */
        public TestDriver2()
        {
            code = new CodeToTest2();
        }
        //----< description of this test >-------------------------------

        public string getLog()
        {
            return "demo a successful test of two integers division: 9/3";
        }
        //----< test method is where all the testing gets done >---------

        public bool test()
        {
            code.annunciator(9, 3);
            return true;
        }
    }

#if (TEST2)
    class Program
    {
        static void Main(string[] args)
        {
            TestDriver2 driver = new TestDriver2();
    
            driver.test();
            Console.Write(driver.getLog());
            Console.ReadKey();

        }
    }
#endif
}
