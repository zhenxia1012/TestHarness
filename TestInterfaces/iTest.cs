/////////////////////////////////////////////////////////////////////
// ITest.cs - define interfaces for test drivers and obj factory   //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarness;

namespace Test
{
    public class dd
    {
        public static void ge()
        {
            int a = 4;

            a += Thread.CurrentThread.ManagedThreadId;

            while (true)
            {
                Thread.Sleep(200);
                Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": a = " + a);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            dd d = new dd();
            Thread td = new Thread(
                () =>
                {
                    dd.ge();
                });
            Thread td1 = new Thread(
                () =>
                {
                    dd.ge();
                });
            td.Start();
            td1.Start();
            


        }
    }
}
