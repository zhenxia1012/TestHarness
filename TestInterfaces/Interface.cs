/////////////////////////////////////////////////////////////////////
// ITest.cs - interfaces for communication between system parts    //
//                                                                 //
// Zhen XiA                                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TestInterface.cs provides interfaces:
 * - ITestHarness   used by TestHarness
 * - ICallback      used by child AppDomain to send messages to TestHarness
 * - IRequestInfo   used by TestHarness
 * - ITestInfo      used by TestHarness
 * - ILoadAndTest   used by TestHarness
 * - TestInterface  sed by LoadAndTest
 * - IRepository    used by Repo
 * - IClient        used by Client
 *
 * Required files:
 * ---------------
 * 
 * Maintanence History:
 * --------------------
 * ver 1.1 : 15 Nov 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to send messages to TestHarness

    public interface ICallback
    {
        void sendMessage(string msg);
    }
    public interface ITestHarness
    {
        void sendMessage(Message msg);
        void sendfile_request(List<string> files, string author, string from);//send Dll request to repository
        void runTests(object tem_msg);//call LoadAndTest to load and run tests
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to invoke test driver's test()

    public interface ITest
    {
        bool test();
        string getLog();
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to communicate with Repository
    // via TestHarness Comm

    public interface IRepository
    {
        bool saveTestfile(byte[] file, string name, string directory);//save Dll files
        bool saveTestResult(TestResults testresults);//save test result and logs sent by TestHarness
        List<Message> getFiles(string path, List<string> fileList, string to);  // return the files TestHarness requires
        void sendMessage(Message msg);
        List<Message> queryLogs(string queryText);// return the logs client query about
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to send results to client
    // via TestHarness Comm

    public interface IClient
    {
        Message makeQuery(string queryText);//support client query about the logs from repository
        List<Message> getFiles(List<string> fileList);//get Dll files from the local repository, make them to be messages for sending them to TestHarness' repository
        bool display_result(TestResults tem_result);//display the test result sent by TestHarness
    }
    /////////////////////////////////////////////////////////////
    // used by TestHarness to communicate with child AppDomain

    public interface ILoadAndTest
    {
        ITestResults test(IRequestInfo requestInfo);// load files from tempory directory and test tests
        void setCallback(ICallback cb);// send message to main Appdmain
        void loadPath(string path);// loacally tempory directory for saving Dll files
    }
    public interface ITestInfo
    {
        string testName { get; set; }
        List<string> files { get; set; }
    }
    public interface IRequestInfo
    {
        List<ITestInfo> requestInfo { get; set; }
    }
    public interface ITestResult
    {
        string testName { get; set; }
        string testResult { get; set; }
        string testLog { get; set; }
    }
    public interface ITestResults
    {
        string testKey { get; set; }
        DateTime dateTime { get; set; }
        List<ITestResult> testResults { get; set; }
    }
}
