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

namespace Remote
{
    public partial class FormRemote : Form
    {
        System.Timers.Timer aTimer = new System.Timers.Timer();
        private bool isExit = false;
        private RemoteData remotedata;
        private ControlData controldata;
        private TcpClient client;
        private IPAddress serverIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork"));
        private IPAddress selectedcontrolIP;
        private NetworkStream networkStream;
        private EventWaitHandle allDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        private delegate void SetListBoxCallback(string str);
        private SetListBoxCallback setListBoxCallback;
        private delegate void DisplayControlDataCallback();
        private DisplayControlDataCallback displayControlDataCallback;

        private delegate void GetRemoteDataCallback();
        private GetRemoteDataCallback getRemoteDataCallback;

        private delegate void GetLufferDataCallback(ref RemoteData remotedata);
        private delegate void GetHeightDataCallback(ref RemoteData remotedata);
        private delegate void GetRotationDataCallback(ref RemoteData remotedata);
        private delegate void DisplayRotationDataCallback(float data);
        private delegate void DisplayLufferDataCallback(float data);
        private delegate void DisplayHeightDataCallback(float data);
        private delegate void DisplayWindspeedDataCallback(float data);
        private delegate void DisplayLeanDataCallback(float data);
        private delegate void DisplayTorqueDataCallback(float data);
        private delegate void Radio1Callback(bool check);
        private delegate void Radio2Callback(bool check);
        private delegate void Radio3Callback(bool check);
        public FormRemote()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false; 
            setListBoxCallback = new SetListBoxCallback(SetListBox);
            displayControlDataCallback = new DisplayControlDataCallback(DisplayControlData);
            getRemoteDataCallback = new GetRemoteDataCallback(GetRemoteData);
            aTimer.Elapsed += new ElapsedEventHandler(aTimer_Elapsed); 
            aTimer.Interval = 500;  //设置时间间隔 
            aTimer.Enabled = false;
            controldata = new ControlData();
            remotedata = new RemoteData();
            button1.Enabled = false;
        }

        private void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                GetRemoteData();
                remotedata.encode();
                consolelist.Invoke(setListBoxCallback, string.Format("发送信息{0}", remotedata.length));
                networkStream.BeginWrite(remotedata.databuffer, 0, remotedata.length, new AsyncCallback(SendCallback), networkStream);
                networkStream.Flush();
            }
            catch(Exception err)
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

        private void button2_Click(object sender, EventArgs e)
        {
            client = new TcpClient(AddressFamily.InterNetwork);
            AsyncCallback requestCallback = new AsyncCallback(RequestCallback);
            allDone.Reset();
            client.BeginConnect(serverIP, 8000, requestCallback, client);
            consolelist.Invoke(setListBoxCallback, string.Format("本机EndPoint：{0}", client.Client.LocalEndPoint));
            consolelist.Invoke(setListBoxCallback, "开始与服务器连接");
            button1.Enabled = true;
            button2.Enabled = false;
            IPcomboBox.Enabled = false;
            allDone.WaitOne();
        }
        private void RequestCallback(IAsyncResult ar)
        {
            allDone.Set();
            try
            {
                client = (TcpClient)ar.AsyncState;
                client.EndConnect(ar);
                consolelist.Invoke(setListBoxCallback, string.Format("与服务器{0}连接成功", client.Client.RemoteEndPoint));
                networkStream = client.GetStream();
                networkStream.BeginRead(controldata.databuffer, 0, controldata.length, ReadCallback, null);

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
                controldata.decode();
                remotedata.data.remoteID = controldata.data.remoteID;
                DisplayControlData();
                if(isExit == false)
                {
                    controldata.Initdata();
                    networkStream.BeginRead(controldata.databuffer, 0, controldata.length, ReadCallback, networkStream);
                }
            }
            catch(Exception err)
            {
                consolelist.Invoke(setListBoxCallback,err.Message);
            }
        }
        private void SetListBox(string str)
        {
            consolelist.Items.Add(str);
            consolelist.SelectedIndex = consolelist.Items.Count - 1;
            consolelist.ClearSelected();
        }
        private void IPcomboBox_DropDown(object sender, EventArgs e)
        {
            IPcomboBox.Items.Clear();
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Ping myPing;
                    myPing = new Ping();
                    myPing.PingCompleted += new PingCompletedEventHandler(_myPing_PingCompleted);
                    string pingIP = "172.31.103." + i.ToString();
                    myPing.SendAsync(pingIP, 1000, null);
                }

            }
            catch (Exception err)
            {
                consolelist.Invoke(setListBoxCallback, err.Message);
            }
        }
        private void IPcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IPcomboBox.SelectedItem != null)
            {
                selectedcontrolIP = IPAddress.Parse(IPcomboBox.SelectedItem.ToString());
                int a = selectedcontrolIP.ToString().LastIndexOf('.');
                remotedata.data.controlID = Convert.ToUInt16(selectedcontrolIP.ToString().Substring(a+1));
            }
        }
        private void _myPing_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string rsl = e.Reply.Address.ToString();
            if (e.Reply.Status == IPStatus.Success)
            {
                this.IPcomboBox.Items.Add(rsl);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            consolelist.Invoke(setListBoxCallback, "onload");
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Ping myPing;
                    myPing = new Ping();
                    myPing.PingCompleted += new PingCompletedEventHandler(_myPing_PingCompleted);
                    string pingIP = "172.31.103." + i.ToString();
                    myPing.SendAsync(pingIP, 1000, null);
                }
            }
            catch(Exception err)
            {
                consolelist.Invoke(setListBoxCallback, err.Message);
            }
        }

        private void startbutton_Click(object sender, EventArgs e)
        {
            remotedata.data.status = 1;
            aTimer.Enabled = true;
        }

        private void stopbutton_Click(object sender, EventArgs e)
        {
            aTimer.Enabled = false;
            remotedata.data.status = 0;
            try
            {
                GetRemoteData();
                remotedata.encode();
                networkStream.BeginWrite(remotedata.databuffer, 0, remotedata.length, new AsyncCallback(SendCallback), networkStream);
                networkStream.Flush();
            }
            catch(Exception err)
            {
                consolelist.Invoke(setListBoxCallback, err.Message);
            }
        }

        private void GetRemoteData()
        {
            GetLufferData(ref remotedata);
            GetHeightData(ref remotedata);
            GetRotationData(ref remotedata);
        }
        private void GetLufferData(ref RemoteData remotedata)
        {
            if(this.luffertrackBar.InvokeRequired)
            {
                GetLufferDataCallback slcb = new GetLufferDataCallback(GetLufferData);
                this.Invoke(slcb, remotedata);
            }
            else
            {
                remotedata.data.luffer = (Int16)luffertrackBar.Value;
            }
        }
        private void GetHeightData(ref RemoteData remotedata)
        {
            if (this.heighttrackBar.InvokeRequired)
            {
                GetHeightDataCallback shcb = new GetHeightDataCallback(GetHeightData);
                this.Invoke(shcb, remotedata);
            }
            else
            {
                remotedata.data.height = (Int16)heighttrackBar.Value;
            }
        }
        private void GetRotationData(ref RemoteData remotedata)
        {
            if (this.rotationtrackBar.InvokeRequired)
            {
                GetRotationDataCallback srcb = new GetRotationDataCallback(GetRotationData);
                this.Invoke(srcb, remotedata);
            }
            else
            {
                remotedata.data.rotation = (Int16)rotationtrackBar.Value;
            }
        }
        private void DisplayControlData()
        {
            DisplayRotationData(controldata.data.rotation);
            DisplayLufferData(controldata.data.luffer);
            DisplayHeightData(controldata.data.height);
            DisplayWindspeedData(controldata.data.windspeed);
            DisplayLeanData(controldata.data.lean);
            DisplayTorqueData(controldata.data.torque);
            switch(controldata.data.remoteID)
            {
                case 0:
                    Radio1(false);
                    Radio2(false);
                    Radio3(false);
                    this.Text = "遥控器未分配";
                    break;
                case 1:
                    Radio1(true);
                    Radio2(false);
                    Radio3(false);
                    this.Text = "遥控器1";
                    break;
                case 2:
                    Radio1(false);
                    Radio2(true);
                    Radio3(false);
                    this.Text = "遥控器2";
                    break;
                case 3:
                    Radio1(false);
                    Radio2(false);
                    Radio3(true);
                    this.Text = "遥控器3";
                    break;
            }

        }
        private void DisplayRotationData(float data)
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
        private void DisplayLufferData(float data)
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
        private void DisplayHeightData(float data)
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
        private void DisplayWindspeedData(float data)
        {
            if (this.windspeedtextBox.InvokeRequired)
            {
                DisplayWindspeedDataCallback display = new DisplayWindspeedDataCallback(DisplayWindspeedData);
                this.Invoke(display, data);
            }
            else
            {
                windspeedtextBox.Text = data.ToString();
            }
        }
        private void DisplayLeanData(float data)
        {
            if (this.leantextBox.InvokeRequired)
            {
                DisplayLeanDataCallback display = new DisplayLeanDataCallback(DisplayLeanData);
                this.Invoke(display, data);
            }
            else
            {
                leantextBox.Text = data.ToString();
            }
        }
        private void DisplayTorqueData(float data)
        {
            if (this.torquetextBox.InvokeRequired)
            {
                DisplayTorqueDataCallback display = new DisplayTorqueDataCallback(DisplayTorqueData);
                this.Invoke(display, data);
            }
            else
            {
                torquetextBox.Text = data.ToString();
            }
        }
        private void Radio1(bool check)
        {
            if (this.radioButton1.InvokeRequired)
            {
                Radio1Callback display = new Radio1Callback(Radio1);
                this.Invoke(display, check);
            }
            else
            {
                radioButton1.Checked = check;
            }
        }
        private void Radio2(bool check)
        {
            if (this.radioButton2.InvokeRequired)
            {
                Radio2Callback display = new Radio2Callback(Radio2);
                this.Invoke(display, check);
            }
            else
            {
                radioButton2.Checked = check;
            }
        }
        private void Radio3(bool check)
        {
            if (this.radioButton3.InvokeRequired)
            {
                Radio3Callback display = new Radio3Callback(Radio3);
                this.Invoke(display, check);
            }
            else
            {
                radioButton3.Checked = check;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isExit = true;
            allDone.Set();
            client.Close();
            button2.Enabled = true;
            button1.Enabled = false;
            IPcomboBox.Enabled = true;
        }
    }
}



 
