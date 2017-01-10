using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;


namespace Control
{
    public partial class Form1 : Form
    {
        System.Timers.Timer aTimer = new System.Timers.Timer();
        private bool isExit = false;
        private RemoteData remotedata;
        private ControlData controldata;
        private TcpClient client;
        private NetworkStream networkStream;
        private IPAddress serverIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork"));
        private IPAddress localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork"));
        private EventWaitHandle allDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        private delegate void SetListBoxCallback(string str);
        private SetListBoxCallback setListBoxCallback;
        private delegate void DisplayControlDataCallback();
        private DisplayControlDataCallback displayControlDataCallback;

        private delegate void DisplayRemoteDataCallback();
        private DisplayRemoteDataCallback displayRemoteDataCallback;

        private delegate void DisplayRotationDataCallback(Int16 data);
        private delegate void DisplayLufferDataCallback(Int16 data);
        private delegate void DisplayHeightDataCallback(Int16 data);
        public Form1()
        {
            InitializeComponent();
            setListBoxCallback = new SetListBoxCallback(SetListBox);
            displayRemoteDataCallback = new DisplayRemoteDataCallback(DisplayRemoteData);
            aTimer.Elapsed += new ElapsedEventHandler(aTimer_Elapsed);
            aTimer.Interval = 500;  //设置时间间隔 
            aTimer.Enabled = false;
            controldata = new ControlData();
            remotedata = new RemoteData();
            int count = localIP.ToString().LastIndexOf('.');
            controldata.data.controlID = Convert.ToUInt16(localIP.ToString().Substring(count+1));
        }
        private void startbutton_Click(object sender, EventArgs e)
        {
            client = new TcpClient(AddressFamily.InterNetwork);
            AsyncCallback requestCallback = new AsyncCallback(RequestCallback);
            allDone.Reset();
            client.BeginConnect(serverIP, 8001, requestCallback, client);
            consolelist.Invoke(setListBoxCallback, string.Format("本机EndPoint：{0}", client.Client.LocalEndPoint));
            consolelist.Invoke(setListBoxCallback, "开始与服务器连接");
            startbutton.Enabled = false;
            stopbutton.Enabled = true;
            allDone.WaitOne();
        }
        private void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                consolelist.Invoke(setListBoxCallback, string.Format("发送信息{0}", controldata.length));
                controldata.encode();
                networkStream.BeginWrite(controldata.databuffer, 0, controldata.length, new AsyncCallback(SendCallback), networkStream);
                networkStream.Flush();
            }
            catch (Exception err)
            {
                consolelist.Invoke(setListBoxCallback, err.Message);
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                networkStream.EndWrite(ar);
            }
            catch (Exception err)
            {
                consolelist.Invoke(setListBoxCallback, err.Message);
            }
        }
        private void RequestCallback(IAsyncResult ar)
        {
            allDone.Set();
            try
            {
                client = (TcpClient)ar.AsyncState;
                client.EndConnect(ar);
                consolelist.Invoke(setListBoxCallback, string.Format("与服务器{0}连接成功", client.Client.RemoteEndPoint));
                aTimer.Enabled = true;
                networkStream = client.GetStream();
                networkStream.BeginRead(remotedata.databuffer, 0, remotedata.length, ReadCallback, null);

            }
            catch (Exception err)
            {
                consolelist.Invoke(setListBoxCallback, err.Message);
                return;
            }
        }
        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                int count = networkStream.EndRead(ar);
                consolelist.Invoke(setListBoxCallback, string.Format("接收信息{0}", count));
                remotedata.decode();
                DisplayRemoteData();
                if (isExit == false)
                {
                    controldata.Initdata();
                    networkStream.BeginRead(remotedata.databuffer, 0, remotedata.length, ReadCallback, networkStream);
                }
            }
            catch (Exception err)
            {
                consolelist.Invoke(setListBoxCallback, err.Message);
            }
        }
        private void SetListBox(string str)
        {
            consolelist.Items.Add(str);
            consolelist.SelectedIndex = consolelist.Items.Count - 1;
            consolelist.ClearSelected();
        }


        private void DisplayRemoteData()
        {
            DisplayRotationData(remotedata.data.rotation);
            DisplayLufferData(remotedata.data.luffer);
            DisplayHeightData(remotedata.data.height);
        }
        private void DisplayRotationData(Int16 data)
        {
            if (this.rotationtextBox.InvokeRequired)
            {
                DisplayRotationDataCallback display = new DisplayRotationDataCallback(DisplayRotationData);
                this.Invoke(display, data);
            }
            else
            {
                rotationtextBox.Text = data.ToString();
            }
        }
        private void DisplayLufferData(Int16 data)
        {
            if (this.luffertextBox.InvokeRequired)
            {
                DisplayLufferDataCallback display = new DisplayLufferDataCallback(DisplayLufferData);
                this.Invoke(display, data);
            }
            else
            {
                luffertextBox.Text = data.ToString();
            }
        }
        private void DisplayHeightData(Int16 data)
        {
            if (this.heighttextBox.InvokeRequired)
            {
                DisplayHeightDataCallback display = new DisplayHeightDataCallback(DisplayHeightData);
                this.Invoke(display, data);
            }
            else
            {
                heighttextBox.Text = data.ToString();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void stopbutton_Click(object sender, EventArgs e)
        {
            allDone.Set();
            client.Close();
            isExit = true;
            startbutton.Enabled = false;
            stopbutton.Enabled = true;
            aTimer.Enabled = false;
        }
    }
}
