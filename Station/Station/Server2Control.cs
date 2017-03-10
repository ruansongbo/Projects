using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Station
{
    class Server2Control
    {
        public TcpClient client;
        public NetworkStream netStream;
        public Server2Remote[] remotelist = new Server2Remote[3];
        private bool isExit;
        private RemoteData remotedata;
        private ControlData controldata;
        public UInt16 controlID;
        public UInt16 incontrolID = 0;
        public delegate void OutputCallback(params object[] args);
        public OutputCallback outputCallback;
        public delegate void DisplayControlDataCallback(ControlData controldata);
        public DisplayControlDataCallback displayControlDataCallback;
        public delegate void ErrorHandleCallback(Server2Control server2control);
        public ErrorHandleCallback errorHandleCallback;

        public Server2Control(TcpClient client, OutputCallback output, DisplayControlDataCallback DisplayControlData,ErrorHandleCallback ErrorHandle)
        {
            this.client = client;
            this.outputCallback = output;
            this.displayControlDataCallback = DisplayControlData;
            this.errorHandleCallback = ErrorHandle;
            netStream = client.GetStream();
            isExit = false;
            controldata = new ControlData();
            remotedata = new RemoteData();
            netStream.BeginRead(controldata.databuffer, 0, controldata.length, ReadCallback, null);
            outputCallback("初始化");
        }
        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                int count = netStream.EndRead(ar);
                controldata.decode();
                for (ushort i = 0; i < 3;i++)
                {
                    if(remotelist[i] != null)
                    {
                        controldata.data.remoteID = (ushort)(i+1);
                        controldata.data.controlID = this.controlID;
                        controldata.data.incontrolID = this.incontrolID;
                        controldata.encode();
                        remotelist[i].SendData(controldata.databuffer);
                    }
                }
                displayControlDataCallback(controldata);
                outputCallback(string.Format("接收信息{0}", count));
                if (isExit == false)
                {
                    controldata.Initdata();
                    netStream.BeginRead(controldata.databuffer, 0, controldata.length, ReadCallback, client);
                }
            }
            catch (Exception err)
            {
                outputCallback(err.Message);
            }
        }
        public void SendData(byte[] databuffer)
        {
            try
            {
                outputCallback(string.Format("发送信息{0}", remotedata.length));
                netStream.BeginWrite(databuffer, 0, remotedata.length, new AsyncCallback(SendCallback), null);
                netStream.Flush();
            }
            catch (Exception err)
            {
                outputCallback(err.Message);
                
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                netStream.EndWrite(ar);
            }
            catch (Exception err)
            {
                outputCallback(err.Message);
            }
        }
        public void ControlExit()
        {
            isExit = true;
        }
    }
}
