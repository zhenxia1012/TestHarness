/////////////////////////////////////////////////////////////////////
// Messages.cs - defines different kinds of message's body         //
// ver 1.0                                                         //
// Zhen Xia                                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Messages provides different kinds of message's body.
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 21 Nov 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class TestResult
    {
        public string testName { get; set; }
        public string testResult { get; set; }
        public string testLog { get; set; }

        public TestResult() { }
    }

    public class TestResults
    {
        public string testKey { get; set; }
        public DateTime dateTime { get; set; }
        public List<TestResult> testResults { get; set; } = new List<TestResult>();

        public TestResults() { }
    }

    public class testElement
    {
        public string testName { get; set; }
        public string testDriver { get; set; }
        public List<string> testCodes { get; set; } = new List<string>();

        public testElement() { }
        public testElement(string name)
        {
            testName = name;
        }
        public void addDriver(string name)
        {
            testDriver = name;
        }
        public void addCode(string name)
        {
            testCodes.Add(name);
        }
        public override string ToString()
        {
            string temp = "<test name=\"" + testName + "\">";
            temp += "<testDriver>" + testDriver + "</testDriver>";
            foreach (string code in testCodes)
                temp += "<library>" + code + "</library>";
            temp += "</test>";
            return temp;
        }
    }

    public class testRequest
    {
        public string author { get; set; }
        public List<testElement> tests { get; set; } = new List<testElement>();

        public testRequest() { }
        public override string ToString()
        {
            string temp = "<testRequest>";
            foreach (testElement te in tests)
                temp += te.ToString();
            temp += "</testRequest>";
            temp = "\n" + temp.formatXml(4);
            return temp;
        }
    }

    public class FileMsg
    {
        public string FileName { get; set; }
        public byte[] block { get; set; }
    }

    public class FileRequest
    {
        public FileRequest() { }
        public FileRequest(string tem_client)
        {
            client = tem_client;
        }
        public string client { get; set; }
        public List<string> files { get; set; } = new List<string>();
    }

    public class Report
    {
        public string report { get; set; }
        public Report() { }
        public Report(string tem_report)
        {
            report = tem_report;
        }
    }

    public class queryMsg
    {
        public string querytext { get; set; }
        public queryMsg() { }
        public queryMsg(string tem_querytext)
        {
            querytext = tem_querytext;
        }
    }
}
