using System;
using System.Text;

namespace Test
{
    public interface ILogger
    {
        ILogger add(string logitem);
        void showCurrentItem();
        void showAll();
        string getlog();
    }

    public class Logger : MarshalByRefObject, ILogger
    {
        private StringBuilder logtext = new StringBuilder();
        private string currentItem = null;

        public Logger()
        {
            string time = DateTime.Now.ToString();
            string title = "\n\n\n  Console Log: " + time;
            logtext = new StringBuilder(title);
            logtext.Append("\n " + new string('=', title.Length));
        }
        public ILogger add(string logitem)
        {
            currentItem = logitem;
            logtext.Append("\n" + logitem);
            return this;
        }
        public void showCurrentItem()
        {
            Console.Write("\n" + currentItem);
        }
        public void showAll()
        {
            Console.Write(logtext + "\n");
        }
        public string getlog()
        {
            return "nothing";
        }
        public string getcontent()
        {
            return logtext.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Logger logger = new Test.Logger();
            logger.showAll();
            Console.Read();
        }
    }
}
