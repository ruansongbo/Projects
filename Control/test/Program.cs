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
using System.Timers;

namespace test
{
    class Program
    {

        private bool isExit = false;
        private TcpClient client;
        private IPAddress selectserver;
        private NetworkStream networkStream;
        private EventWaitHandle allDone = new EventWaitHandle(false, EventResetMode.ManualReset);

        static void Main(string[] args)
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Interval = 500;  //设置时间间隔 
            aTimer.Enabled = false;

            ControlData controldata = new ControlData();
            RemoteData remotedata = new RemoteData();
        }

    }
}
