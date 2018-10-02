using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    ///////////////////////////////////////////////////////////////////
    // Test and RequestInfo are used to pass test request information
    // to child AppDomain
    //
    [Serializable]
    public class Test : ITestInfo
    {
        public string testName { get; set; }
        public List<string> files { get; set; } = new List<string>();
    }
    [Serializable]
    public class RequestInfo : IRequestInfo
    {
        public string tempDirName { get; set; }
        public List<ITestInfo> requestInfo { get; set; } = new List<ITestInfo>();
    }
    [Serializable]
    public class TestResultInfo : ITestResult
    {
        public string testName { get; set; }
        public string testResult { get; set; }
        public string testLog { get; set; }
    }
    [Serializable]
    public class TestResultsInfo : ITestResults
    {
        public string testKey { get; set; }
        public DateTime dateTime { get; set; }
        public List<ITestResult> testResults { get; set; } = new List<ITestResult>();
    }
}
