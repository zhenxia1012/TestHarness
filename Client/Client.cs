/////////////////////////////////////////////////////////////////////
// Client.cs - an interface for user to use TestHarness            //
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WCF;

namespace TestHarness
{
    public class Client : IClient
    {
        public Comm<Client> Comm_Client;
        string client_address = Comm<Client>.makeEndPoint("http://localhost", 8080);
        string th_address = Comm<Client>.makeEndPoint("http://localhost", 8081);
        string repo_address = Comm<Client>.makeEndPoint("http://localhost", 8082);
        public BlockingQueue<Message> inQ_ { get; set; } = new BlockingQueue<Message>();
        private string Root = "./Client/CRepo";
        object sync_ = new object();
        public List<Thread> threads_ = new List<Thread>();
        //----<constructor>----------------------------------------------

        public Client()
        {
            Comm_Client = new Comm<Client>();
            Comm_Client.rcvr.CreateRecvChannel(client_address);
            Comm_Client.rcvr.start(thrdProc);
        }
        //----<thrdProc() is used for deliver message to Client >--------

        public void thrdProc()
        {
            while (true)
            {
                Message msg = Comm_Client.rcvr.GetMessage();
                if (msg.body == "quit")
                    continue;
                inQ_.enQ(msg);
            }
        }
        //----<activate sender>------------------------------------------

        public void senderstart()
        {
            Comm_Client.sndr.start();
        }
        //----<send message>---------------------------------------------

        public void sendMessage(Message msg)
        {
            Comm_Client.sndr.PostMessage(msg);
        }
        //----<close sender>---------------------------------------------

        public void senderclose()
        {
            Comm_Client.sndr.Close();
        }
        //----< get files and make them to be messages >-----------------

        public List<Message> getFiles(List<string> fileList)
        {
            string path = Path.Combine(Root,"TestInstance");
            List<Message> files = new List<Message>();
            foreach (string file in fileList)
            {
                string filepath = Path.Combine(path, file);
                Message tem_msg = Message.makeMessage("File", "Zhen Xia", repo_address, client_address, filepath);
                if (tem_msg.body == null)
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": get file \"" + file + "\" failed");
                else
                    files.Add(tem_msg);
            }
            return files;
        }
        //----< make test request >--------------------------------------

        public Message makeTestRequest(List<string> fileList)
        {
            Message testrequest = Message.makeMessage("TestRequest", "Zhen Xia", th_address, client_address, fileList);
            return testrequest;
        }
        //----< make query about log >-----------------------------------

        public Message makeQuery(string queryText)
        {
            queryMsg tem_qry = new queryMsg(queryText);
            Message tem_msg = Message.makeMessage("Query", "Zhen Xia", repo_address, client_address, tem_qry);
            return tem_msg;
        }
        //----< Save the files >-----------------------------------

        public bool savefile(byte[] file, string name, string directory)
        {
            string file_path = Path.Combine(directory, name);
            try
            {
                using (var savestream = new FileStream(file_path, FileMode.Create))
                {
                    savestream.Write(file, 0, file.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not save file \"" + name + "\"");
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + ex.Message);
                return false;
            }
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": save file \"" + name + "\"");
            return true;
        }
        //----< display test result >------------------------------------

        public bool display_result(TestResults tem_result)
        {
            Console.WriteLine("\n  " + tem_result.testKey);
            foreach (TestResult test in tem_result.testResults)
            {
                Console.WriteLine("\n  -----------------------------");
                Console.WriteLine("\n  " + test.testName);
                Console.WriteLine("\n  " + test.testResult);
                Console.WriteLine("\n  " + test.testLog);
            }
            Console.WriteLine("\n  -----------------------------");
            return true;
        }
        //----< quit Message >-------------------------------------------

        public Message makQuit(string dst)
        {
            Message quit = new Message();
            quit.type = Message.totaltype.Quit;
            quit.author = "Zhen Xia";
            quit.to = dst;
            quit.from = client_address;
            quit.time = DateTime.Now;
            quit.body = "quit";
            return quit;
        }
        //----< process Message >----------------------------------------

        public void processMsg(object obj_msg)
        {
            Message msg = (Message)obj_msg;
            switch (msg.type.ToString())
            {
                case "Report":
                    {
                        Report tem_body = (Report)msg.parse_body();
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + tem_body.report.ToString());
                    }
                    break;
                case "File":
                    {
                        FileMsg tem_body = (FileMsg)msg.parse_body();
                        string tem_path = Path.Combine(Root, "TestResult");
                        savefile(tem_body.block, tem_body.FileName, tem_path);
                        tem_path = Path.Combine(tem_path, tem_body.FileName);
                        StreamReader sr = new StreamReader(tem_path, Encoding.Default);
                        Console.WriteLine("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": \n");
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Console.WriteLine("  " + line.ToString());
                        }
                    }
                    break;
                case "TestResult":
                    {
                        TestResults tem_body = (TestResults)msg.parse_body();
                        display_result(tem_body);
                    }
                    break;
            }
            threads_.Remove(Thread.CurrentThread);
            Thread.CurrentThread.Abort();
        }
        //----< create thread to deal message >---------------------------

        public void ConfigureClient()
        {
            int max_numOfthreads = 1;
            while (true)
            {
                if (threads_.Count < max_numOfthreads)
                {
                    Message msg = inQ_.deQ();
                    ParameterizedThreadStart ps = processMsg;
                    Thread td = new Thread(processMsg);
                    threads_.Add(td);
                    td.Start(msg);
                    Thread.Sleep(100);
                }
                else
                    Thread.Sleep(500);
            }
        }

        public void checkQueue()
        {
            Thread td = new Thread(ConfigureClient);
            td.Start();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.checkQueue();
            Message quit_to_th = client.makQuit(Comm<Client>.makeEndPoint("http://localhost", 8081));
            Message quit_to_repo = client.makQuit(Comm<Client>.makeEndPoint("http://localhost", 8082));

            /*List<string> files = new List<string> { "TestCode1.dll", "TestDriver1.dll", "TestCode2.dll", "TestDriver2.dll" };
            List<Message> msgs = client.getFiles(files);
            client.senderstart();
            foreach (Message tem_msg in msgs)
            {
                client.sendMessage(tem_msg);
            }
            client.sendMessage(quit_to_repo);
            client.Comm_Client.sndr.join();
            //client.senderclose();
            Console.ReadKey();*/

            List<string> tests = new List<string> { "test1"};
            Message msg1 = client.makeTestRequest(tests);
            msg1.show();
            client.senderstart();
            client.sendMessage(msg1);
            //client.sendMessage(msg1);
            client.sendMessage(quit_to_th);
            client.Comm_Client.sndr.join();
            //client.senderclose();
            //Console.ReadKey();

            List<string> tests1 = new List<string> { "test2"};
            Message msg3 = client.makeTestRequest(tests1);
            msg3.show();
            client.senderstart();
            client.sendMessage(msg3);
            client.sendMessage(quit_to_th);
            client.Comm_Client.sndr.join();
            //client.senderclose();
            Console.ReadKey();

            Message msg2 = client.makeQuery("2016");
            client.senderstart();
            client.sendMessage(msg2);
            client.sendMessage(quit_to_repo);
            client.Comm_Client.sndr.join();
            //client.senderclose();
            Console.Write("\n  Press key to quit");
            Console.ReadKey();
        }
    }
}
