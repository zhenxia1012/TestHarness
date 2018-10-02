/////////////////////////////////////////////////////////////////////
// Messages.cs - defines communication messages                    //
// ver 2.0                                                         //
// Zhen Xia                                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Messages provides helper code for building and parsing XML messages.
 *
 * Required files:
 * ---------------
 * - Messages.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 16 Oct 2016
 * - first release
 * ver 2.0 : 21 Nov 2016
 * - add functionalities making message and parsing message body
 */
#define TEST_MESSAGES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace TestHarness
{
    [Serializable]
    public class Message
    {
        public enum totaltype { Quit = 0, TestRequest = 1, TestResult = 2, File = 3, FileRequest = 4, Report = 5, Query }
        public totaltype type;
        public string to { get; set; }
        public string from { get; set; }
        public string author { get; set; }
        public DateTime time { get; set; }
        public string body { get; set; }

        public Message(int typenum = 0)
        {
            type = (totaltype)typenum;
            to = "";
            from = "";
            author = "";
            time = DateTime.Now;
            body = "";
        }
        public Message fromString(string msgStr)
        {
            Message msg = new Message();
            try
            {
                string[] parts = msgStr.Split(',');
                for (int i = 0; i < parts.Count(); ++i)
                    parts[i] = parts[i].Trim();

                msg.to = parts[0].Substring(4);
                msg.from = parts[1].Substring(6);
                msg.type = (totaltype)Enum.Parse(typeof(totaltype), parts[2].Substring(6), true);
                msg.author = parts[3].Substring(8);
                msg.time = DateTime.Parse(parts[4].Substring(6));
                if (parts[5].Count() > 6)
                    msg.body = parts[5].Substring(6);
            }
            catch
            {
                Console.Write("\n  string parsing failed in Message.fromString(string)");
                return null;
            }
            return msg;
        }
        public override string ToString()
        {
            string temp = "to: " + to;
            temp += ", from: " + from;
            temp += ", type: " + type.ToString();
            if (author != "")
                temp += ", author: " + author;
            temp += ", time: " + time;
            temp += ", body:\n" + body;
            return temp;
        }
        public Message copy(Message msg)
        {
            Message temp = new Message();
            temp.to = msg.to;
            temp.from = msg.from;
            temp.type = msg.type;
            temp.author = msg.author;
            temp.time = DateTime.Now;
            temp.body = msg.body;
            return temp;
        }
        public static Message makeMessage(string type, string author, string to, string from, object body)
        {
            Message tem_msg = new Message();
            tem_msg.type = (totaltype)Enum.Parse(typeof(totaltype), type, true);
            tem_msg.to = to;
            tem_msg.from = from;
            tem_msg.author = author;

            switch (type)
            {
                case "TestRequest":
                    {
                        testRequest tr = new testRequest();
                        tr.author = author;

                        List<string> files = (List<string>)body;
                        foreach (string file in files)
                        {
                            testElement te = new testElement(file);
                            te.addDriver("TestDriver" + file.Substring(4) + ".dll");
                            te.addCode("TestCode" + file.Substring(4) + ".dll");
                            tr.tests.Add(te);
                        }

                        tem_msg.body = tr.ToXml();
                    }
                    break;
                case "File":
                    {
                        string path = (string)body;
                        FileMsg tem_testmsg = new FileMsg();
                        tem_testmsg.FileName = Path.GetFileName(path);
                        FileStream tem_stream = null;
                        try
                        {
                            tem_stream = new FileStream(path, FileMode.Open);
                            tem_testmsg.block = new byte[tem_stream.Length];
                            tem_stream.Read(tem_testmsg.block, 0, tem_testmsg.block.Length);
                            tem_stream.Flush();
                            tem_stream.Close();
                            tem_msg.body = tem_testmsg.ToXml();
                        }
                        catch
                        {
                            tem_msg.body = null;
                        }
                    }
                    break;
                case "TestResult":
                    {
                        TestResults ts = (TestResults)body;
                        tem_msg.body = ts.ToXml();
                    }
                    break;
                case "FileRequest":
                    {
                        FileRequest fr = (FileRequest)body;
                        tem_msg.body = fr.ToXml();
                    }
                    break;
                case "Report":
                    {
                        Report rp = (Report)body;
                        tem_msg.body = rp.ToXml();
                    }
                    break;
                case "Query":
                    {
                        queryMsg qry = (queryMsg)body;
                        tem_msg.body = qry.ToXml();
                    }
                    break;
            }

            return tem_msg;
        }
    }

    public static class extMethods
    {
        public static void show(this Message msg, int shift = 2)
        {
            Console.Write("\n  formatted message:");
            string[] lines = msg.ToString().Split(',');
            foreach (string line in lines)
                Console.Write("\n    {0}", line.Trim());
            Console.WriteLine();
        }
        public static object parse_body(this Message msg)
        {
            object tem_body = new object();
            switch (msg.type.ToString())
            {
                case "TestRequest":
                    {
                        testRequest testrq_body = msg.body.FromXml<testRequest>();
                        tem_body = testrq_body;
                    }
                    break;
                case "File":
                    {
                        FileMsg testmsg_body = msg.body.FromXml<FileMsg>();
                        tem_body = testmsg_body;
                    }
                    break;
                case "TestResult":
                    {
                        TestResults testrs_body = msg.body.FromXml<TestResults>();
                        tem_body = testrs_body;
                    }
                    break;
                case "FileRequest":
                    {
                        FileRequest fr = msg.body.FromXml<FileRequest>();
                        tem_body = fr;
                    }
                    break;
                case "Report":
                    {
                        Report rp = msg.body.FromXml<Report>();
                        tem_body = rp;
                    }
                    break;
                case "Query":
                    {
                        queryMsg qry = msg.body.FromXml<queryMsg>();
                        tem_body = qry;
                    }
                    break;
            }
            return tem_body;
        }
        public static string parse_header(this Message msg)
        {
            string temp_header = "to: " + msg.to;
            temp_header += ", from: " + msg.from;
            temp_header += ", type: " + msg.type.ToString();
            if (msg.author != "")
                temp_header += ", author: " + msg.author;
            temp_header += ", time: " + msg.time;
            return temp_header;
        }
        public static string shift(this string str, int n = 2)
        {
            string insertString = new string(' ', n);
            string[] lines = str.Split('\n');
            for (int i = 0; i < lines.Count(); ++i)
            {
                lines[i] = insertString + lines[i];
            }
            string temp = "";
            foreach (string line in lines)
                temp += line + "\n";
            return temp;
        }
        public static string formatXml(this string xml, int n = 2)
        {
            XDocument doc = XDocument.Parse(xml);
            return doc.ToString().shift(n);
        }
    }

#if (TEST_MESSAGES)
    class TestMessages
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Message Class");
            Console.Write("\n =======================\n");

            Message msg = new Message();
            msg.to = "TH";
            msg.from = "CL";
            msg.type = Message.totaltype.TestRequest;
            msg.author = "Fawcett";
            msg.body = "    a body";

            Console.Write("\n  base message:\n    {0}", msg.ToString());
            Console.WriteLine();
            msg.show();
            Console.WriteLine();

            Console.Write("\n  Testing Message.fromString(string)");
            Console.Write("\n ------------------------------------");
            Message parsed = msg.fromString(msg.ToString());
            parsed.show();
            Console.WriteLine();
            Console.ReadKey();
        }
    }
#endif

}
