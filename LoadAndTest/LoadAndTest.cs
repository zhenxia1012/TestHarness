/////////////////////////////////////////////////////////////////////
// LoadAndTest.cs - loads and executes tests using reflection      //
// ver1.0                                                          //
// Author: ZhenXia                                                 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * LoadAndTest package operates in child AppDomain.  It loads and
 * executes test code defined by a TestRequest message.
 *
 * Required files:
 * ---------------
 * - LoadAndTest.cs
 * - ITest.cs
 * - TestHarness.cs
 * - Logger.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 17 Nov 2016
 * - first release
 * - change the part of code of loading files so that the exception 
 *   of failing to load file could be caught
 */
//#define TEST_LOADANDTEST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.Threading;

namespace TestHarness
{
    public class LoadAndTest : MarshalByRefObject, ILoadAndTest
    {
        private ICallback cb_ = null;
        private string loadPath_ = "";
        object sync_ = new object();
        TestResultsInfo testResults_ = new TestResultsInfo();

        //----< initialize loggers >-------------------------------------

        public LoadAndTest()
        {
        }
        //----< set loadPath >-------------------------------------------

        public void loadPath(string path)
        {
            loadPath_ = path;
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": loadpath = {0}", loadPath_);
        }
        //----< load libraries into child AppDomain and test >-----------

        public ITestResults test(IRequestInfo testRequest)
        {
            foreach (ITestInfo test in testRequest.requestInfo)
            {
                TestResultInfo testResult = new TestResultInfo();
                testResult.testName = test.testName;
                try
                {
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": -- \"" + test.testName + "\" --");

                    ITest tdr = null;
                    string testDriverName = "";

                    foreach (string file in test.files)
                    {
                        Assembly assem = null;
                        try
                        {
                            if (loadPath_.Count() > 0)
                            {
                                for (int i = 0; i < 5; ++i)
                                {
                                    try
                                    {
                                        assem = Assembly.LoadFrom(loadPath_ + "/" + file);
                                        break;
                                    }
                                    catch
                                    {
                                        //Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": Loading failed for {0} times", i+1);
                                        Thread.Sleep(100);
                                    }
                                }
                                assem = Assembly.LoadFrom(loadPath_ + "/" + file);
                            }
                            else
                                assem = Assembly.Load(file);
                        }
                        catch
                        {
                            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": can't load\"" + file + "\"");
                            continue;
                        }
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": loaded \"" + file + "\"");
                        Type[] types = assem.GetExportedTypes();

                        foreach (Type t in types)
                        {
                            if (t.IsClass && typeof(ITest).IsAssignableFrom(t))  // does this type derive from ITest ?
                            {
                                try
                                {
                                    testDriverName = file;
                                    tdr = (ITest)Activator.CreateInstance(t);    // create instance of test driver
                                    Console.Write(
                                      "\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + testDriverName + " implements ITest interface - Req #5"
                                    );
                                }
                                catch
                                {
                                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ":----" + file + " - exception thrown when created");
                                    continue;
                                }
                            }
                        }
                    }
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": testing " + testDriverName);
                    bool testReturn;
                    try
                    {
                        testReturn = tdr.test();
                    }
                    catch
                    {
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ":----exception thrown when testing");
                        testReturn = false;
                    }
                    if (tdr != null && testReturn == true)
                    {
                        testResult.testResult = "passed";
                        testResult.testLog = tdr.getLog();
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": test passed");
                        if (cb_ != null)
                        {
                            cb_.sendMessage(testDriverName + " passed");
                        }
                    }
                    else
                    {
                        testResult.testResult = "failed";
                        if (tdr != null)
                            testResult.testLog = tdr.getLog();
                        else
                            testResult.testLog = "file not loaded";
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": test failed");
                        if (cb_ != null && tdr != null)
                        {
                            cb_.sendMessage(testDriverName + ": failed");
                        }
                        else
                        {
                            cb_.sendMessage("test driver not loaded");
                        }
                    }
                }
                catch (Exception ex)
                {
                    testResult.testResult = "failed";
                    testResult.testLog = "exception thrown";
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + ex.Message);
                }
                testResults_.testResults.Add(testResult);
            }

            testResults_.dateTime = DateTime.Now;
            testResults_.testKey = System.IO.Path.GetFileName(loadPath_);
            return testResults_;
        }
        //----< TestHarness calls to pass ref to Callback function >-----

        public void setCallback(ICallback cb)
        {
            cb_ = cb;
        }

#if (TEST_LOADANDTEST)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing LoaderAndTest");
            Console.Write("\n-------------------------------");
            Test test1 = new Test();
            test1.testName = "test1";
            test1.files.Add("TestCode1.dll");
            test1.files.Add("TestDriver1.dll");
            Test test2 = new Test();
            test2.testName = "test2";
            test2.files.Add("TestCode2.dll");
            test2.files.Add("TestDriver2.dll");
            RequestInfo testrequest = new RequestInfo();
            testrequest.tempDirName = "testrequest";
            testrequest.requestInfo.Add(test1);
            testrequest.requestInfo.Add(test2);

            LoadAndTest loader = new LoadAndTest();
            loader.loadPath("../../../TestHarness/THRepo/Zhen Xia_2016_11_18_20_45_33.9692860_ThreadID10");
            loader.test(testrequest);
            Console.ReadKey();

        }
#endif
    }
}
