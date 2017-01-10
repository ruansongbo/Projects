using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace test
{
    class Program
    {
        class MyTime
        {
            public System.Timers.Timer mytime = new System.Timers.Timer();
            private void test(object source, System.Timers.ElapsedEventArgs e)
            {
                int i = 0;
                Console.WriteLine("{0}", i);
                i++;
            }
            public void set()
            {
                mytime.Elapsed += new System.Timers.ElapsedEventHandler(test);
                mytime.Interval = 1000;
                mytime.AutoReset = true;
                mytime.Enabled = true;
            }
        }

        static void Main(string[] args)
        {
            MyTime tt = new MyTime();
            tt.set();
        }
    }
}
