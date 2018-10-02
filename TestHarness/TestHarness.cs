/////////////////////////////////////////////////////////////////////
// TestHarness.cs - TestHarness Engine: creates child domains      //
// ver 1.0                                                         //
// Zhen Xia                                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TestHarness package provides integration testing services.  It:
 * - receives structured test requests
 * - retrieves cited files from a repository
 * - executes tests on all code that implements an ITest interface,
 *   e.g., test drivers.
 * - reports pass or fail status for each test in a test request
 * - stores test logs in the repository
 * It contains classes:
 * - TestHarness that runs all tests in child AppDomains
 * - Callback to support sending messages from a child AppDomain to
 *   the TestHarness primary AppDomain.
 * - Test and RequestInfo to support transferring test information
 *   from TestHarness to child AppDomain
 * 
 * Required Files:
 * ---------------
 * - TestHarness.cs
 * - BlockingQueue.cs
 * - TestInterface.cs
 * - LoadAndTest.cs, Logger.cs, Messages.cs
 *
 * Maintanence History:
 * --------------------
 * ver 2.1 : 15 Nov 2016
 * - removed logger test due to race condition in logger - will fix later
 * - Added custom thread local storage so that lock in runTests function
 *   could be removed.  
 * ver 2.0 : 13 Nov 2016
 * - added creation of threads to run tests in ProcessMessages
 * - removed logger statements as they were confusing with multiple threads
 * - added locking in a few places
 * - added more error handling
 * - No longer save temp directory name in member data of TestHarness class.
 *   It's now captured in TestResults data structure.
 * ver 1.1 : 11 Nov 2016
 * - added ability for test harness to pass a load path to
 *   LoadAndTest instance in child AppDomain
 * ver 1.0 : 16 Oct 2016
 * - first release
 */
#define TEST_TESTHARNESS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Policy;    // defines evidence needed for AppDomain construction
using System.Runtime.Remoting;   // provides remote communication between AppDomains
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.IO;
using System.Security.AccessControl;
using WCF;

namespace TestHarness
{
    ///////////////////////////////////////////////////////////////////
    // Callback class is used to receive messages from child AppDomain
    //
    public class Callback : MarshalByRefObject, ICallback
    {
        public void sendMessage(string msg)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": received msg from childDomain: \"" + msg + "\"");
        }
    }
    ///////////////////////////////////////////////////////////////////
    // class TestHarness
    //
    public class TestHarness : ITestHarness
    {
        Comm<TestHarness> Comm_TH;
        string repo_address = Comm<TestHarness>.makeEndPoint("http://localhost", 8082);
        string th_address = Comm<TestHarness>.makeEndPoint("http://localhost", 8081);
        string client_address = Comm<TestHarness>.makeEndPoint("http://localhost", 8080);
        public BlockingQueue<Message> inQ_ { get; set; } = new BlockingQueue<Message>();
        private ICallback cb_;
        private string filePath_;
        object sync_ = new object();
        List<Thread> threads_ = new List<Thread>();
        Dictionary<int, string> TLS = new Dictionary<int, string>();
        //----<constructor>------------------------------------------

        public TestHarness()
        {
            Console.Write("\n  creating instance of TestHarness");
            cb_ = new Callback();
            Comm_TH = new Comm<TestHarness>();
            Comm_TH.rcvr.CreateRecvChannel(th_address);
            Comm_TH.rcvr.start(thrdProc);
        }
        //----<thrdProc() is used for deliver message to TestHarness >---

        public void thrdProc()
        {
            while (true)
            {
                Message msg = Comm_TH.rcvr.GetMessage();
                //msg.show();
                if (msg.body == "quit")
                    continue;
                inQ_.enQ(msg);
            }
        }
        //----<activate sender>------------------------------------------

        public void senderstart()
        {
            Comm_TH.sndr.start();
        }
        //----< send message>--------------------------------------------

        public void sendMessage(Message msg)
        {
            Comm_TH.sndr.PostMessage(msg);
        }
        //----<close sender>---------------------------------------------

        public void senderclose()
        {
            Comm_TH.sndr.Close();
        }
        //----< make path name from author and time >--------------------

        string makeKey(string author)
        {
            DateTime now = DateTime.Now;
            string nowDateStr = now.Date.ToString("d");
            string[] dateParts = nowDateStr.Split('/');
            string key = "";
            foreach (string part in dateParts)
                key += part.Trim() + '_';
            string nowTimeStr = now.TimeOfDay.ToString();
            string[] timeParts = nowTimeStr.Split(':');
            for (int i = 0; i < timeParts.Count() - 1; ++i)
                key += timeParts[i].Trim() + '_';
            key += timeParts[timeParts.Count() - 1];
            key = author + "_" + key + "_" + "ThreadID" + Thread.CurrentThread.ManagedThreadId;
            return key;
        }
        //----< quit Message >-------------------------------------------

        Message makeQuit(string dst)
        {
            Message quit = new Message();
            quit.type = Message.totaltype.Quit;
            quit.author = "Repository";
            quit.to = dst;
            quit.from = th_address;
            quit.time = DateTime.Now;
            quit.body = "quit";
            return quit;
        }
        //----< retrieve test information from testRequest >-------------

        List<ITestInfo> extractTests(Message testRequest)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": parsing test request");
            testRequest tem_testrq = (testRequest)testRequest.parse_body();

            List<ITestInfo> tests = new List<ITestInfo>();
            XDocument doc = XDocument.Parse(tem_testrq.ToString());
            foreach (XElement testElem in doc.Descendants("test"))
            {
                Test test = new Test();
                string testDriverName = testElem.Element("testDriver").Value;
                test.testName = testElem.Attribute("name").Value;
                test.files.Add(testDriverName);
                foreach (XElement lib in testElem.Elements("library"))
                {
                    test.files.Add(lib.Value);
                }
                tests.Add(test);
            }
            return tests;
        }
        //----< retrieve test code from testRequest >--------------------

        List<string> extractCode(List<ITestInfo> testInfos)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": retrieving code files from testInfo data structure");
            List<string> codes = new List<string>();
            foreach (ITestInfo testInfo in testInfos)
                codes.AddRange(testInfo.files);
            return codes;
        }
        //----< create local directory>----------------------------------

        RequestInfo processRequest(Message testRequest)
        {
            string localDir_ = "./TestHarness/THRepo/" + makeKey(testRequest.author);// name of temporary dir to hold test files
            RequestInfo rqi = new RequestInfo();
            rqi.requestInfo = extractTests(testRequest);

            rqi.tempDirName = localDir_;
            lock (sync_)
            {
                filePath_ = System.IO.Path.GetFullPath(localDir_);  // LoadAndTest will use this path
                TLS[Thread.CurrentThread.ManagedThreadId] = filePath_;
            }
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": creating local test directory:\n     \"" + localDir_ + "\"");
            Directory.CreateDirectory(localDir_);
            return rqi;
        }
        //----< send requests of asking files>--------------------------

        public void sendfile_request(List<string> files, string author, string from)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": send message of asking files");
            FileRequest tem_fr = new FileRequest(author);
            tem_fr.files = files;
            Message filerequest = Message.makeMessage("FileRequest", "Zhen Xia", repo_address, from, tem_fr);
            lock (sync_)
            {
                senderstart();
                sendMessage(filerequest);
                sendMessage(makeQuit(repo_address));
                Comm_TH.sndr.join();
                //senderclose();
            }
        }
        //----< load files from repository>-----------------------------

        public void loadfiles(BlockingQueue<Message> RP_to_TH, int numOffiles, string destpath)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": loading code from Repository");
            // check whehter the required files is received
            int current_rcv = 0;
            while (true)
            {
                if (current_rcv == numOffiles)
                    break;

                Message tem_testfile;
                tem_testfile = RP_to_TH.deQ();
                if (tem_testfile.type.ToString() == "Report")
                {
                    Report tem_error = (Report)tem_testfile.parse_body();
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + tem_error.report.ToString()+ " - Req #3");
                }
                else
                {
                    FileMsg tem_testmsg = (FileMsg)tem_testfile.parse_body();
                    byte[] block = tem_testmsg.block;
                    try
                    {
                        FileStream tem_stream = new FileStream(destpath + "/" + tem_testmsg.FileName, FileMode.Create, FileAccess.Write);
                        tem_stream.Write(block, 0, block.Length);
                        tem_stream.Flush();
                        tem_stream.Close();
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": retrieved file \"" + tem_testmsg.FileName + "\"");
                    }
                    catch
                    {
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not retrieve file \"" + tem_testmsg.FileName + "\"");
                    }
                }
                current_rcv++;
            }
            Console.WriteLine();
        }
        //----< save results and logs in Repository >--------------------

        TestResults makeTestRsMsg(ITestResults testResults)
        {
            TestResults tem_tss = new TestResults();
            tem_tss.testKey = testResults.testKey;
            tem_tss.dateTime = testResults.dateTime;

            foreach (ITestResult ts in testResults.testResults)
            {
                TestResult tem_ts = new TestResult();
                tem_ts.testName = ts.testName;
                tem_ts.testResult = ts.testResult;
                tem_ts.testLog = ts.testLog;
                tem_tss.testResults.Add(tem_ts);
            }
            return tem_tss;
        }
        //----< run tests >----------------------------------------------

        public void runTests(object tem_msg)
        {
            BlockingQueue<Message> RP_to_TH = new BlockingQueue<Message>();
            Comm<AppDomain> child_td = new Comm<AppDomain>();
            string child_address = Comm<AppDomain>.makeEndPoint("http://localhost", 8100 + Thread.CurrentThread.ManagedThreadId);
            child_td.rcvr.CreateRecvChannel(child_address);
            child_td.rcvr.start(() =>
                {
                    while (true)
                    {
                        Message msg = child_td.rcvr.GetMessage();
                        if (msg.body == "quit")
                            continue;
                        //msg.show();
                        RP_to_TH.enQ(msg);
                    }
                });

            Message testRequest = (Message)tem_msg;
            AppDomain ad = null;
            ILoadAndTest ldandtst = null;
            RequestInfo rqi = null;
            ITestResults tr = null;

            try
            {
                rqi = processRequest(testRequest);
                List<string> files = extractCode(rqi.requestInfo);
                sendfile_request(files, testRequest.author, child_address);
                loadfiles(RP_to_TH, files.Count, rqi.tempDirName);
                ad = createChildAppDomain();
                ldandtst = installLoader(ad);
                if (ldandtst != null)
                {
                    tr = ldandtst.test(rqi);
                }

                lock (sync_)// unloading ChildDomain, and so unloading the library
                {
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": unloading: \"" + ad.FriendlyName + "\"\n");
                    AppDomain.Unload(ad);
                    try
                    {
                        Thread.Sleep(3000);
                        Directory.Delete(rqi.tempDirName, true);
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": removed directory " + rqi.tempDirName);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not remove directory " + rqi.tempDirName);
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ":\n\n---- {0}\n\n", ex.Message);
            }
            
            TestResults tem_results = makeTestRsMsg(tr);
            Message result_to_repo = Message.makeMessage("TestResult", "TestHarness", repo_address, child_address, tem_results);
            Message result_to_client = Message.makeMessage("TestResult", "TestHarness", client_address, child_address, tem_results); ;

            //send result to client
            lock (sync_)
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": Send result to Repository - Req#7");
                child_td.sndr.start();
                child_td.sndr.PostMessage(result_to_repo);
                child_td.sndr.PostMessage(makeQuit(repo_address));
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": Send result to Client - Req#7");
                child_td.sndr.start();
                child_td.sndr.PostMessage(result_to_client);
                child_td.sndr.PostMessage(makeQuit(client_address));
                child_td.sndr.join();
                //child_td.sndr.Close();
            }

            child_td.rcvr.abort();
            threads_.Remove(Thread.CurrentThread);
            Thread.CurrentThread.Abort();
        }
        //----< main activity of TestHarness >---------------------------

        public void processMessages()
        {
            AppDomain main = AppDomain.CurrentDomain;
            int max_numofthread = 1;
            Console.Write("\n  Starting in AppDomain " + main.FriendlyName + "\n");

            ParameterizedThreadStart doTests = new ParameterizedThreadStart(runTests);

            while (true)
            {
                if (threads_.Count < max_numofthread)
                {
                    Message testRequest = inQ_.deQ();
                    Console.Write("\n  Receving test requests from clients - Req #2");
                    testRequest.show();
                    Console.Write("\n  Creating AppDomain thread");
                    Thread t = new Thread(doTests);
                    threads_.Add(t);
                    t.Start(testRequest);
                    Thread.Sleep(100);
                }
                else
                    Thread.Sleep(500);
            }
        }
        //----< was used for debugging >---------------------------------

        void showAssemblies(AppDomain ad)
        {
            Assembly[] arrayOfAssems = ad.GetAssemblies();
            foreach (Assembly assem in arrayOfAssems)
                Console.Write("\n  " + assem.ToString());
        }
        //----< create child AppDomain >---------------------------------

        public AppDomain createChildAppDomain()
        {
            try
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": creating child AppDomain - Req #4");

                AppDomainSetup domaininfo = new AppDomainSetup();
                domaininfo.ApplicationBase
                  = "file:///" + "./TestHarness/bin/Debug";  // defines search path for LoadAndTest library

                //Create evidence for the new AppDomain from evidence of current
                Evidence adevidence = AppDomain.CurrentDomain.Evidence;

                // Create Child AppDomain
                AppDomain ad
                  = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);

                Console.Write("\n  created AppDomain \"" + ad.FriendlyName + "\"");
                return ad;
            }
            catch (Exception except)
            {
                Console.Write("\n  " + except.Message + "\n\n");
            }
            return null;
        }
        //----< Load and Test is responsible for testing >---------------

        ILoadAndTest installLoader(AppDomain ad)
        {
            ad.Load("LoadAndTest");

            // create proxy for LoadAndTest object in child AppDomain
            ObjectHandle oh
              = ad.CreateInstance("LoadAndTest", "TestHarness.LoadAndTest");
            object ob = oh.Unwrap();    // unwrap creates proxy to ChildDomain

            // set reference to LoadAndTest object in child
            ILoadAndTest landt = (ILoadAndTest)ob;

            // create Callback object in parent domain and pass reference
            // to LoadAndTest object in child
            landt.setCallback(cb_);
            lock (sync_)
            {
                filePath_ = TLS[Thread.CurrentThread.ManagedThreadId];
                landt.loadPath(filePath_);  // send file path to LoadAndTest
            }
            return landt;
        }

#if (TEST_TESTHARNESS)
        static void Main(string[] args)
        {
            TestHarness th = new TestHarness();
            th.processMessages();
            Console.ReadKey();
        }
#endif
    }
}
