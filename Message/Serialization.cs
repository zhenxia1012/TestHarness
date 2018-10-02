///////////////////////////////////////////////////////////////////////
// TestXmlSerialization.cs - Demonstrate XML Serializer              //
//                                                                   //
// Zhen XIa                                                          //
///////////////////////////////////////////////////////////////////////
/*
 * Demonstrate serializing an instance of some type to an XML string
 * and deserialize back to an instance of that type.
 *
 */
//#define TEST_SERIALIZATION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

namespace TestHarness
{
    public static class ToAndFromXml
    {
        //----< serialize object to XML >--------------------------------

        static public string ToXml(this object obj)
        {
            // suppress namespace attribute in opening tag

            XmlSerializerNamespaces nmsp = new XmlSerializerNamespaces();
            nmsp.Add("", "");

            var sb = new StringBuilder();
            try
            {
                var serializer = new XmlSerializer(obj.GetType());
                using (StringWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, obj, nmsp);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": exception thrown:");
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": {0}", ex.Message);
            }
            return sb.ToString();
        }
        //----< deserialize XML to object >------------------------------

        static public T FromXml<T>(this string xml)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(new StringReader(xml));
            }
            catch (Exception ex)
            {
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": deserialization failed\n  {0}", ex.Message);
                return default(T);
            }
        }
    }

    public static class Utilities
    {
        public static void title(this string aString, char underline = '-')
        {
            Console.Write("\n {0}", aString);
            Console.Write("\n {0}", new string(underline, aString.Length + 2));
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Widget class - used to test serialization and deserialization
    //
    public class Widget
    {
        public Widget() { }
        public Widget(string nm)
        {
            name = nm;
        }
        public string getName()
        {
            return name;
        }
        public string name { get; set; }
    }

#if (TEST_SERIALIZATION)
    class TestXmlSerialization
    {
        static void Main(string[] args)
        {
            "Demonstrating XML Serialization and Deserialization".title('=');
            Console.WriteLine();

            Console.Write("\n  attempting to serialize Widget object:");
            Widget widget = new Widget("Jim");
            string xml = widget.ToXml();
            if (xml.Count() > 0)
                Console.Write("\n  widget:\n{0}\n", xml);
            Console.ReadKey();
            Console.Write("\n  attempting to deserialize Widget object:");
            Widget newWidget = xml.FromXml<Widget>();
            if (newWidget == null)
            {
                Console.Write("\n  deserialized object is null");
            }
            else
            {
                string type = newWidget.GetType().Name;
                Console.Write("\n  retrieved object of type: {0}", type);

                string test = newWidget.getName();
                Console.Write("\n  reconstructed widget's name = \"{0}\"", test);
            }
            Console.Write("\n\n");
            Console.ReadKey();
        }
    }
#endif
}
