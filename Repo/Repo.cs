/////////////////////////////////////////////////////////////////////
// Repo.cs - act as a database for saving data of user and test    //
// ver1.0                                                          //
// Author: ZhenXia                                                 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * 
 *
 * Required files:
 * ---------------
 * - BlockingQueue.cs
 * - Communication.cs
 * - Message.cs
 * - TestInterface.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 17 Nov 2016
 * - first release
 */
#define TEST_REPO
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarness;
using WCF;

namespace TestHarness
{
    public class Repo : IRepository
    {
        Comm<Repo> Comm_Repo;
        string repo_address = Comm<Repo>.makeEndPoint("http://localhost", 8082);
        string th_address = Comm<Repo>.makeEndPoint("http://localhost", 8081);
        string client_address = Comm<Repo>.makeEndPoint("http://localhost", 8080);
        public BlockingQueue<Message> inQ_ { get; set; } = new BlockingQueue<Message>();
        public static string Root = "./Repo/Repository";
        object sync_ = new object();
        List<Thread> threads_ = new List<Thread>();
        //----<constructor>----------------------------------------------

        public Repo()
        {
            Comm_Repo = new Comm<Repo>();
            Comm_Repo.rcvr.CreateRecvChannel(repo_address);
            Comm_Repo.rcvr.start(thrdProc);
        }
        //----<thrdProc() is used for deliver message to Repository >---

        public void thrdProc()
        {
            while (true)
            {
                Message msg = Comm_Repo.rcvr.GetMessage();
                if (msg.body == "quit")
                    continue;
                inQ_.enQ(msg);
            }
        }
        //----<activate sender>------------------------------------------

        public void senderstart()
        {
            Comm_Repo.sndr.start();
        }
        //----<send message>---------------------------------------------

        public void sendMessage(Message msg)
        {
            Comm_Repo.sndr.PostMessage(msg);
        }
        //----<close sender>---------------------------------------------

        public void senderclose()
        {
            Comm_Repo.sndr.Close();
        }
        //----< Save the test files >-----------------------------------

        public bool saveTestfile(byte[] file, string name, string directory)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": saving file \"" + name + "\"");
            string file_path = Path.Combine(Root, "TestInstance", directory);
            if (!Directory.Exists(file_path))
                Directory.CreateDirectory(file_path);

            file_path = Path.Combine(file_path, name);
            try
            {
                using (var savestream = new FileStream(file_path, FileMode.Create))
                {
                    savestream.Write(file, 0, file.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not save file \"" + name + "\": \n      " + ex.Message);
                return false;
            }
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": save file \"" + name + "\"");
            return true;
        }
        //----< Save the test result >-----------------------------------

        public bool saveTestResult(TestResults testresults)
        {
            string logName = testresults.testKey + ".txt";
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": saving result \"" + logName + "\" - Req#8");
            string tem_directory = Path.Combine(Root, "TestResult");
            if (!Directory.Exists(tem_directory))
                Directory.CreateDirectory(tem_directory);

            StreamWriter sr = null;
            try
            {
                sr = new StreamWriter(Path.Combine(tem_directory, logName));
                sr.WriteLine(logName);
                foreach (TestResult test in testresults.testResults)
                {
                    sr.WriteLine("-----------------------------");
                    sr.WriteLine(test.testName);
                    sr.WriteLine(test.testResult);
                    sr.WriteLine(test.testLog);
                }
                sr.WriteLine("-----------------------------");
                sr.Close();
            }
            catch (Exception ex)
            {
                sr.Close();
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not save file \"" + testresults.testKey + "\": \n      " + ex.Message);
                return false;
            }
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": save result \"" + testresults.testKey + "\"");
            return true;
        }
        //----< get files >----------------------------------------------

        public List<Message> getFiles(string path, List<string> fileList, string to)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": retreive files.");
            List<Message> files = new List<Message>();
            foreach (string file in fileList)
            {
                string filepath = Path.Combine(path, file);
                Message tem_msg = Message.makeMessage("File", "Repository", to, repo_address, filepath);
                if (tem_msg.body == null)
                {
                    Report report = new Report(" The file \"" + file + "\" does not exist in repository");
                    Message error = Message.makeMessage("Report", "Repository", to, repo_address, report);
                    files.Add(error);
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": The file \"" + file + "\" does not exist in repository");
                }
                else
                {
                    files.Add(tem_msg);
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": The file \"" + file + "\" exists in repository");
                }
            }
            return files;
        }

        //----< search for text in log files >---------------------------

        public List<Message> queryLogs(string queryText)
        {
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": find logs which contain \"" + queryText + "\" - Req#9");
            List<Message> queryResults = new List<Message>();
            string path = Path.Combine(Root, "TestResult");
            string[] files = Directory.GetFiles(path, "*.txt");
            foreach (string file in files)
            {
                string contents = File.ReadAllText(file);
                if (contents.Contains(queryText))
                {
                    Message logmsg = Message.makeMessage("File", "Repository", client_address, repo_address, file);
                    queryResults.Add(logmsg);
                }
            }
            return queryResults;
        }
        //----< quit Message >-------------------------------------------

        public Message makeQuit(string dst)
        {
            Message quit = new Message();
            quit.type = Message.totaltype.Quit;
            quit.author = "Repository";
            quit.to = dst;
            quit.from = repo_address;
            quit.time = DateTime.Now;
            quit.body = "quit";
            return quit;
        }
        //----< process Message >----------------------------------------

        public void processMsg(object obj_msg)
        {
            Message msg = (Message)obj_msg;
            Message quit = makeQuit(msg.from);
            switch (msg.type.ToString())
            {
                case "File":
                    {
                        FileMsg tem_body = (FileMsg)msg.parse_body();
                        Report tem_report_body = null;
                        if (saveTestfile(tem_body.block, tem_body.FileName, msg.author))
                            tem_report_body = new Report("saving file \"" + tem_body.FileName + "\" in repository succeeded");
                        else
                            tem_report_body = new Report("saving file \"" + tem_body.FileName + "\" in repository failed");
                        Message report = Message.makeMessage("Report", "Repository", client_address, repo_address, tem_report_body);
                        lock (sync_)
                        {
                            senderstart();
                            sendMessage(report);
                            sendMessage(quit);
                            Comm_Repo.sndr.join();
                            //senderclose();
                        }
                    }
                    break;
                case "TestResult":
                    {
                        TestResults tem_body = (TestResults)msg.parse_body();
                        saveTestResult(tem_body);
                    }
                    break;
                case "FileRequest":
                    {
                        FileRequest tem_body = (FileRequest)msg.parse_body();
                        string tem_directory = Path.Combine(Root, "TestInstance", tem_body.client);
                        List<Message> tem_msgs = getFiles(tem_directory, tem_body.files, msg.from);
                        lock (sync_)
                        {
                            Comm_Repo.sndr.start();
                            foreach (Message tem_msg in tem_msgs)
                                sendMessage(tem_msg);
                            sendMessage(quit);
                            Comm_Repo.sndr.join();
                            //senderclose();
                        }
                    }
                    break;
                case "Query":
                    {
                        queryMsg tem_body = (queryMsg)msg.parse_body();
                        string tem_text = msg.author + "_" + tem_body.querytext;
                        List<Message> files = queryLogs(tem_text);
                        lock (sync_)
                        {
                            Comm_Repo.sndr.start();
                            if (files.Count == 0)
                            {
                                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": The files which contain \"" + tem_body.querytext + "\" don't exist!");
                                Report tem_rp = new Report(" The files which contain \"" + tem_body.querytext + "\" don't exist!");
                                Message tem_msg = Message.makeMessage("Report", "Repository", client_address, repo_address, tem_rp);
                                Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": The files which contain \"" + tem_body.querytext + "\" don't exist!");
                                sendMessage(tem_msg);
                            }
                            else
                                foreach (Message tem_msg in files)
                                {
                                    sendMessage(tem_msg);
                                }
                            sendMessage(quit);
                            Comm_Repo.sndr.join();
                            //senderclose();
                        }
                    }
                    break;
            }
            threads_.Remove(Thread.CurrentThread);
            Thread.CurrentThread.Abort();
        }
        //----< create thread to deal message >---------------------------

        public void ConfigureRepo()
        {
            int max_numOfthreads = 1;
            while (true)
            {
                if (threads_.Count < max_numOfthreads)
                {
                    Message msg = inQ_.deQ();
                    Console.Write("\n dequeue a message of " + msg.type.ToString());
                    ParameterizedThreadStart ps = processMsg;
                    Thread td = new Thread(processMsg);
                    threads_.Add(td);
                    Console.Write("\n create a thread TID" + td.ManagedThreadId + " for message of " + msg.type.ToString());
                    td.Start(msg);
                    Thread.Sleep(100);
                }
                else
                    Thread.Sleep(500);
            }
        }
    }

#if (TEST_REPO)
    class Program
    {
        static void Main(string[] args)
        {
            Repo repo = new Repo();
            repo.ConfigureRepo();
            Console.Write("\n  Press key to quit");
            Console.ReadKey();
        }
    }
#endif
}
