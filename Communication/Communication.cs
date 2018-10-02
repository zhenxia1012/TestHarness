/////////////////////////////////////////////////////////////////////
// CommService.cs - Communicator Service                           //
// ver 1.0                                                         //
// Zhen Xia                                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defindes a Sender class and Receiver class that
 * manage all of the details to set up a WCF channel.
 * 
 * Required Files:
 * ---------------
 * CommService.cs, ICommunicator, BlockingQueue.cs, Messages.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 25 Nov 2016
 * - first release
 */
#define TEST_COMMSERVICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using TestHarness;

namespace WCF
{
    ///////////////////////////////////////////////////////////////////
    // Receiver hosts Communication service used by other Peers

    public class Receiver<T> : ICommunicator
    {
        static BlockingQueue<Message> rcvBlockingQ = null;
        ServiceHost service = null;
        Thread td;

        public string name { get; set; }

        public Receiver()
        {
            if (rcvBlockingQ == null)
                rcvBlockingQ = new BlockingQueue<Message>();
        }

        public void start(ThreadStart rcvThreadProc)
        {
            td = new Thread(rcvThreadProc);
            td.Start();
        }

        public void abort()
        {
            td.Abort();
        }

        public void Close()
        {
            service.Close();
        }

        public void CreateRecvChannel(string address)
        {
            try
            {
                WSHttpBinding binding = new WSHttpBinding();
                Uri baseAddress = new Uri(address);
                service = new ServiceHost(typeof(Receiver<T>), baseAddress);
                service.AddServiceEndpoint(typeof(ICommunicator), binding, baseAddress);
                service.Open();
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": Service is open listening on {0} - Req#10", address);
            }
            catch (Exception ex)
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": Service opening failed on \"{0}\": \n    {1}", address, ex.Message);
            }
        }

        public void PostMessage(Message msg)
        {
            rcvBlockingQ.enQ(msg);
        }

        public Message GetMessage()
        {
            Message msg = rcvBlockingQ.deQ();
            return msg;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Sender is client of another Peer's Communication service

    public class Sender
    {
        public string name { get; set; }

        ICommunicator channel;
        string lastError = "";
        BlockingQueue<Message> sndBlockingQ = null;
        Thread sndThrd = null;
        int tryCount = 0, MaxCount = 10;
        string currEndpoint = "";
        //----< processing for send thread >-----------------------------

        void ThreadProc()
        {
            tryCount = 0;
            while (true)
            {
                Message msg = sndBlockingQ.deQ();
                if (msg.to != currEndpoint)
                {
                    currEndpoint = msg.to;
                    CreateSendChannel(currEndpoint);
                }
                while (true)
                {
                    try
                    {
                        channel.PostMessage(msg);
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": posted message from {0} to {1}", name, msg.to);
                        tryCount = 0;
                        break;
                    }
                    catch 
                    {
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": connection failed");
                        if (++tryCount < MaxCount)
                            Thread.Sleep(100);
                        else
                        {
                            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": {0}", "can't connect\n");
                            currEndpoint = "";
                            tryCount = 0;
                            break;
                        }
                    }
                }
                if (msg.body == "quit")
                    break;
            }
            Thread.CurrentThread.Abort();
        }
        //----< initialize Sender >--------------------------------------

        public Sender()
        {
            sndBlockingQ = new BlockingQueue<Message>();
        }
        //----< activate sender >----------------------------------------

        public  void start ()
        {
            sndThrd = new Thread(ThreadProc);
            sndThrd.IsBackground = true;
            sndThrd.Start();
        }
        //----< sndThrd Join >-------------------------------------------

        public void join()
        {
            sndThrd.Join();
        }
        //----< Create proxy to another Peer's Communicator >------------

        public void CreateSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, address);
            channel = factory.CreateChannel();
            Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": service proxy created for {0} - Req#10", address);
        }
        //----< posts message to another Peer's queue >------------------

        public void PostMessage(Message msg)
        {
            sndBlockingQ.enQ(msg);
        }

        public string GetLastError()
        {
            string temp = lastError;
            lastError = "";
            return temp;
        }
        //----< closes the send channel >--------------------------------

        public void Close()
        {
            ChannelFactory<ICommunicator> temp = (ChannelFactory<ICommunicator>)channel;
            temp.Close();
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Comm class simply aggregates a Sender and a Receiver
    //
    public class Comm<T>
    {
        public string name { get; set; } = typeof(T).Name;

        public Receiver<T> rcvr { get; set; } = new Receiver<T>();

        public Sender sndr { get; set; } = new Sender();

        public Comm()
        {
            rcvr.name = name;
            sndr.name = name;
        }
        public static string makeEndPoint(string url, int port)
        {
            string endPoint = url + ":" + port.ToString() + "/ICommunicator";
            return endPoint;
        }
    }

    class Cat { }
    class TestComm
    {
        [STAThread]
        static void Main(string[] args)
        {
#if (TEST_COMMSERVICE)
            Comm<Cat> comm = new Comm<Cat>();
            string endPoint = Comm<Cat>.makeEndPoint("http://localhost", 8080);
            comm.rcvr.CreateRecvChannel(endPoint);
            comm.rcvr.start(()=> 
            {
                while (true)
                {
                    Message msg = comm.rcvr.GetMessage();
                    msg.show();
                    if (msg.body == "quit")
                    {
                        break;
                    }
                }
            });
            comm.sndr = new Sender();
            comm.sndr.CreateSendChannel(endPoint);
            comm.sndr.start();
            Message msg1 = new Message();
            msg1.body = "Message #1";
            msg1.to = endPoint;
            comm.sndr.PostMessage(msg1);
            Message msg2 = new Message();
            msg2.body = "quit";
            msg2.to = endPoint;
            comm.sndr.PostMessage(msg2);
            comm.sndr.join();
            comm.sndr.Close();
            Console.Write("\n  Comm Service Running:");
            Console.Write("\n  Press key to quit");
            Console.ReadKey();
            Console.Write("\n\n");
#endif
        }
    }
    }
