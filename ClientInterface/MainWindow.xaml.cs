using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestHarness;
using WCF;

namespace ClientInterface
{
    public static class extend
    {
        public static string format(this string text)
        {
            string tem_text = text + "\n--------------------------------------------";
            return tem_text;
        }
    }

    public class test
    {
        public test(string tem_name) { name = tem_name; }
        public string name { get; set; }
    }

    public class file
    {
        public file(string tem_name) { name = tem_name; }
        public string name { get; set; }
    }

    public partial class MainWindow : Window
    {
        Client client;
        Message quit_to_th;
        Message quit_to_repo;
        List<file> files;
        List<test> tests;
        delegate void NewMessage(string msg);
        event NewMessage OnNewMessage;
        private string Root = "./Client/CRepo";
        enum connectype { none = 1, TestRequest = 1, TestFile = 2, Query = 3 }
        connectype current_connect = connectype.none;
        Message tr = null;
        Message query = null;
        //----< called by UI thread when dispatched from rcvThrd >-------

        void OnNewMessageHandler(string msg)
        {
            result.Items.Insert(result.Items.Count, msg);
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
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, OnNewMessage, tem_body.report.format());
                    }
                    break;
                case "File":
                    {
                        FileMsg tem_body = (FileMsg)msg.parse_body();
                        string tem_path = System.IO.Path.Combine(Root, "TestResult");
                        client.savefile(tem_body.block, tem_body.FileName, tem_path);
                        tem_path = System.IO.Path.Combine(tem_path, tem_body.FileName);
                        StreamReader sr = new StreamReader(tem_path, Encoding.Default);
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, OnNewMessage, line.ToString());
                        }
                    }
                    break;
                case "TestResult":
                    {
                        TestResults tem_body = (TestResults)msg.parse_body();
                        string tem_result = "";
                        tem_result += "\n" + tem_body.testKey;
                        foreach (TestResult test in tem_body.testResults)
                        {
                            tem_result += "\n-----------------------------";
                            tem_result += "\n" + test.testName;
                            tem_result += "\n" + test.testResult;
                            tem_result += "\n" + test.testLog;
                        }
                        tem_result += "\n-----------------------------";
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, OnNewMessage, tem_result);
                    }
                    break;
            }
            client.threads_.Remove(Thread.CurrentThread);
            Thread.CurrentThread.Abort();
        }
        //----< create thread to deal message >---------------------------

        public void ConfigureClient()
        {
            int max_numOfthreads = 1;
            while (true)
            {
                if (client.threads_.Count < max_numOfthreads)
                {
                    Message msg = client.inQ_.deQ();
                    ParameterizedThreadStart ps = processMsg;
                    Thread td = new Thread(processMsg);
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

        private void initialize_client()
        {
            client = new Client();
            checkQueue();
            files = new List<file>();
            tests = new List<test>();
            quit_to_th = client.makQuit(Comm<Client>.makeEndPoint("http://localhost", 8081));
            quit_to_repo = client.makQuit(Comm<Client>.makeEndPoint("http://localhost", 8082));

            string[] tem_files = Directory.GetFiles("./Client/CRepo/TestInstance", "*.dll");
            foreach (string tem_file in tem_files)
            {
                string filename = System.IO.Path.GetFileName(tem_file);
                files.Add(new file(filename));
                string test = "test" + filename.Substring(filename.Length - 5, 1);
                bool isexist = false;
                foreach (test tem_test in tests)
                    if (tem_test.name == test)
                    {
                        isexist = true;
                        break;
                    }
                if (!isexist)
                    tests.Add(new test(test));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            connect.IsEnabled = false;
            disconnect.IsEnabled = false;
            send.IsEnabled = false;
            generate.IsEnabled = false;
            yeartext.IsEnabled = false;
            monthtext.IsEnabled = false;
            daytext.IsEnabled = false;

            OnNewMessage += new NewMessage(OnNewMessageHandler);
            initialize_client();
        }

        private void sendll_Click(object sender, RoutedEventArgs e)
        {
            connect.IsEnabled = true;
            disconnect.IsEnabled = false;
            send.IsEnabled = false;
            generate.IsEnabled = false;
            yeartext.IsEnabled = false;
            monthtext.IsEnabled = false;
            daytext.IsEnabled = false;

            current_connect = connectype.TestFile;

            while (menu.Items.Count > 0)
                menu.Items.RemoveAt(0);

            foreach (file tem_file in files)
            {
                CheckBox checkitem = new CheckBox();
                checkitem.Content = tem_file.name;
                menu.Items.Add(checkitem);
            }
        }

        private void sendtestrequest_Click(object sender, RoutedEventArgs e)
        {
            connect.IsEnabled = true;
            disconnect.IsEnabled = false;
            send.IsEnabled = false;
            generate.IsEnabled = true;
            yeartext.IsEnabled = false;
            monthtext.IsEnabled = false;
            daytext.IsEnabled = false;

            current_connect = connectype.TestRequest;

            while (menu.Items.Count > 0)
                menu.Items.RemoveAt(0);

            foreach (test tem_test in tests)
            {
                CheckBox checkitem = new CheckBox();
                checkitem.Content = tem_test.name;
                menu.Items.Add(checkitem);
            }
        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            connect.IsEnabled = true;
            disconnect.IsEnabled = false;
            send.IsEnabled = false;
            generate.IsEnabled = false;
            yeartext.IsEnabled = true;
            monthtext.IsEnabled = true;
            daytext.IsEnabled = true;

            current_connect = connectype.Query;

            while (menu.Items.Count > 0)
                menu.Items.RemoveAt(0);
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            client.senderstart();
            result.Items.Insert(result.Items.Count, "Succeed in connecting for " + current_connect.ToString().format());

            send.IsEnabled = true;
            disconnect.IsEnabled = true;
            connect.IsEnabled = false;
        }

        private void disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (current_connect.ToString() == "Query" || current_connect.ToString() == "TestFile")
                client.sendMessage(quit_to_repo);
            else if (current_connect.ToString() == "TestRequest")
                client.sendMessage(quit_to_th);
            result.Items.Insert(result.Items.Count, "disconnect for " + current_connect.ToString().format());

            send.IsEnabled = false;
            disconnect.IsEnabled = false;
            connect.IsEnabled = true;
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {

            if (current_connect.ToString() == "TestFile")
            {
                List<string> names = new List<string>();
                for (int i = 0; i < menu.Items.Count; i++)
                {
                    CheckBox tem_box = (CheckBox)menu.Items[i];
                    if (tem_box.IsChecked == false)
                        continue;
                    names.Add(tem_box.Content.ToString());
                }
                List<Message> file_msgs = client.getFiles(names);
                foreach (Message msg in file_msgs)
                    client.sendMessage(msg);
            }
            else if (current_connect.ToString() == "TestRequest")
            {
                if (tr != null)
                {
                    client.sendMessage(tr);
                    result.Items.Insert(result.Items.Count, "Sent test request to TestHarness".format());
                }
                else
                    result.Items.Insert(result.Items.Count, "You need to generate a test request first".format());
            }
            else if (current_connect.ToString() == "Query")
            {
                string tem = yeartext.Text.ToString() + "_" + monthtext.Text.ToString() + "_" + daytext.Text.ToString();
                string[] tem1 = tem.Split('_');
                string querytext = "";
                foreach (string time in tem1)
                    if (time != "")
                        querytext += time + "_";
                querytext = querytext.Substring(0,querytext.Length - 1);
                query = client.makeQuery(querytext);
                client.sendMessage(query);
                result.Items.Insert(result.Items.Count, "Sent query text \""+querytext+"\" to repository".format());
            }
        }

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < menu.Items.Count; i++)
            {
                CheckBox tem_box = (CheckBox)menu.Items[i];
                if (tem_box.IsChecked == false)
                    continue;
                names.Add(tem_box.Content.ToString());
            }

            tr = client.makeTestRequest(names);
            testRequest tem_tr = (testRequest)tr.parse_body();
            result.Items.Insert(result.Items.Count, "--------------------------------------------");
            result.Items.Insert(result.Items.Count, tem_tr.ToString().format());
        }

    }
}
