using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Station
{
    class Server2Remote
    {
        public TcpClient client;
        public NetworkStream netStream;
        Server2Control[] controllist;
        private bool isExit;
        private RemoteData remotedata;
        private ControlData controldata;
        public ushort remoteID = 0;
        public delegate void OutputCallback(params object[] args);
        public OutputCallback outputCallback;
        public delegate void DisplayRemoteDataCallback(RemoteData remotedata);
        public DisplayRemoteDataCallback displayRemoteDataCallback;
        public Server2Remote(TcpClient client, Server2Control[] controllist, OutputCallback output, DisplayRemoteDataCallback DisplayRemoteData)
        {
            this.client = client;
            this.controllist = controllist;
            this.outputCallback = output;
            this.displayRemoteDataCallback = DisplayRemoteData;
            netStream = client.GetStream();
            isExit = false;
            controldata = new ControlData();
            remotedata = new RemoteData();
            netStream.BeginRead(remotedata.databuffer, 0, remotedata.length, ReadCallback, null);
            outputCallback("初始化");
        }
        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                int count = netStream.EndRead(ar);            
                remotedata.decode();
                if (controllist[remotedata.data.controlID] != null)
                {
                    if(remoteID == 0)
                    {
                        if (controllist[remotedata.data.controlID].remotelist[0] == null)
                        {
                            controllist[remotedata.data.controlID].remotelist[0] = this;
                            remoteID = 1;
                        }
                        else if (controllist[remotedata.data.controlID].remotelist[1] == null)
                        {
                            controllist[remotedata.data.controlID].remotelist[1] = this;
                            remoteID = 2;
                        }
                        else if (controllist[remotedata.data.controlID].remotelist[2] == null)
                        {
                            controllist[remotedata.data.controlID].remotelist[2] = this;
                            remoteID = 3;
                        }                  
                    } 
                    else
                    {                    
                        if (remoteID == controllist[remotedata.data.controlID].incontrolID || controllist[remotedata.data.controlID].incontrolID == 0)
                        {
                            if (remotedata.data.status == 1)
                            {
                                remotedata.data.remoteID = remoteID;
                                remotedata.encode();
                                controllist[remotedata.data.controlID].SendData(remotedata.databuffer);
                                controllist[remotedata.data.controlID].incontrolID = remoteID;
                            }
                            else
                            {
                                controllist[remotedata.data.controlID - 1].incontrolID = 0;
                            }
                        }
                    } 
                }           
                displayRemoteDataCallback(remotedata);
                outputCallback(string.Format("接收信息{0}",count));
                if(isExit == false)
                {
                    remotedata.Initdata();
                    netStream.BeginRead(remotedata.databuffer, 0, remotedata.length, ReadCallback, null);
                }
            }
            catch(Exception err)
            {
                outputCallback(err.Message);
            }
        }
        public void SendData(byte[] buffer)
        {
            try
            {
                outputCallback(string.Format("发送信息{0}", controldata.length));
                netStream.BeginWrite(buffer, 0, controldata.length, new AsyncCallback(SendCallback), null);
                netStream.Flush();
            }
            catch(Exception err)
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
            catch(Exception err)
            {
                outputCallback(err.Message);
            }
        }
        public void RemoteExit()
        {
            isExit = true;
        }
    }
}
