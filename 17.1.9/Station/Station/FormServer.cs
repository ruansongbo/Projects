using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Station
{
    public partial class FormServer : Form
    {
        TcpListener remoteListener, controlListener;
        Server2Control[] controllist = new Server2Control[10];
        EventWaitHandle remoteAllDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle controlAllDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        private bool isExit = false;
        private delegate void SetListBoxCallback(string str);
        private SetListBoxCallback setListBoxCallback;
        private delegate void Exit();
        private Exit exit;

        private delegate void DisplayRLufferDataCallback(Int16 data);
        private delegate void DisplayRHeightDataCallback(Int16 data);
        private delegate void DisplayRRotationDataCallback(Int16 data);
        private delegate void DisplayPLufferDataCallback(Int16 data);
        private delegate void DisplayPHeightDataCallback(Int16 data);
        private delegate void DisplayPRotationDataCallback(Int16 data);

        private delegate void DisplayRotationDataCallback(float data);
        private delegate void DisplayLufferDataCallback(float data);
        private delegate void DisplayHeightDataCallback(float data);
        private delegate void DisplayWindspeedDataCallback(float data);
        private delegate void DisplayLeanDataCallback(float data);
        private delegate void DisplayTorqueDataCallback(float data);
        private void ToExit()
        {
            isExit = true;
        }
        public FormServer()
        {
            InitializeComponent();
            setListBoxCallback = new SetListBoxCallback(SetListBox);
            exit = new Exit(ToExit);
        }

        private void startbutton_Click(object sender, EventArgs e)
        {
            ThreadStart remoteTS = new ThreadStart(AcceptRemoteConnect);
            Thread remoteThread = new Thread(remoteTS);
            remoteThread.Start();
            ThreadStart controlTS = new ThreadStart(AcceptControlConnect);
            Thread controlThread = new Thread(controlTS);
            controlThread.Start();
            startbutton.Enabled = false;
        }
        private void AcceptRemoteConnect()
        {
            IPAddress ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork"));
            remoteListener = new TcpListener(ip, 8000);
            remoteListener.Start();
            while(isExit == false)
            {
                try
                {
                    remoteAllDone.Reset();
                    AsyncCallback callback = new AsyncCallback(AcceptRemoteCallback);
                    console.Invoke(setListBoxCallback, "开始遥控端连接");
                    remoteListener.BeginAcceptTcpClient(callback, remoteListener);
                    remoteAllDone.WaitOne();

                }
                catch(Exception err)
                {
                    console.Invoke(setListBoxCallback, err.Message);
                    break;
                }
            }
        }
        private void AcceptControlConnect()
        {
            IPAddress ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork"));
            controlListener = new TcpListener(ip, 8001);
            controlListener.Start();
            while (isExit == false)
            {
                try
                {
                    controlAllDone.Reset();
                    AsyncCallback callback = new AsyncCallback(AcceptControlCallback);
                    console.Invoke(setListBoxCallback, "开始主机端连接");
                    controlListener.BeginAcceptTcpClient(callback, controlListener);
                    controlAllDone.WaitOne();
                }
                catch (Exception err)
                {
                    console.Invoke(setListBoxCallback, err.Message);
                    break;
                }
            }
        }
        private void AcceptRemoteCallback(IAsyncResult ar)
        {
            try
            {
                remoteAllDone.Set(); ;
                TcpListener mylistener = (TcpListener)ar.AsyncState;
                TcpClient client = mylistener.EndAcceptTcpClient(ar);
                console.Invoke(setListBoxCallback, "已接受遥控连接" + client.Client.RemoteEndPoint);
                Server2Remote remote = new Server2Remote(client, controllist, Output, DisplayRemote);
                exit += remote.RemoteExit;
            } 
            catch(Exception err)
            {
                console.Invoke(setListBoxCallback,err.Message);
                return;
            }
        }
        private void AcceptControlCallback(IAsyncResult ar)
        {
            try
            {
                controlAllDone.Set();
                TcpListener mylistener = (TcpListener)ar.AsyncState;
                TcpClient client = mylistener.EndAcceptTcpClient(ar);
                console.Invoke(setListBoxCallback, "已接受主机连接" + client.Client.RemoteEndPoint);
                Server2Control control = new Server2Control(client, Output, DisplayControl);
                int count = client.Client.RemoteEndPoint.ToString().LastIndexOf('.');
                UInt16 index = Convert.ToUInt16(client.Client.RemoteEndPoint.ToString().Substring(count+1,1));
                control.controlID = index;
                controllist[index] = control;
                exit += control.ControlExit;
            }
            catch (Exception err)
            {
                console.Invoke(setListBoxCallback, err.Message);
                return;
            }
        }
        private void SetListBox(string str)
        {
            console.Items.Add(str);
            console.SelectedIndex = console.Items.Count - 1;
            console.ClearSelected();
        }
        private void DisplayRemote(RemoteData remotedata)
        {
            DisplayRLufferData(remotedata.data.luffer);
            DisplayRHeightData(remotedata.data.height);
            DisplayRRotationData(remotedata.data.rotation);
            DisplayPLufferData(remotedata.data.luffer);
            DisplayPHeightData(remotedata.data.height);
            DisplayPRotationData(remotedata.data.rotation);
        }
        private void DisplayRRotationData(Int16 data)
        {
            if (this.RrotationtextBox.InvokeRequired)
            {
                DisplayRRotationDataCallback display = new DisplayRRotationDataCallback(DisplayRRotationData);
                this.Invoke(display, data);
            }
            else
            {
                RrotationtextBox.Text = data.ToString();
            }
        }
        private void DisplayRLufferData(Int16 data)
        {
            if (this.RluffertextBox.InvokeRequired)
            {
                DisplayRLufferDataCallback display = new DisplayRLufferDataCallback(DisplayRLufferData);
                this.Invoke(display, data);
            }
            else
            {
                RluffertextBox.Text = data.ToString();
            }
        }
        private void DisplayRHeightData(Int16 data)
        {
            if (this.RheighttextBox.InvokeRequired)
            {
                DisplayRHeightDataCallback display = new DisplayRHeightDataCallback(DisplayRHeightData);
                this.Invoke(display, data);
            }
            else
            {
                RheighttextBox.Text = data.ToString();
            }
        }
        private void DisplayPRotationData(Int16 data)
        {
            if (this.rotationprogressBar.InvokeRequired)
            {
                DisplayPRotationDataCallback display = new DisplayPRotationDataCallback(DisplayPRotationData);
                this.Invoke(display, data);
            }
            else
            {
                rotationprogressBar.Value = data + 10;
            }
        }
        private void DisplayPLufferData(Int16 data)
        {
            if (this.lufferprogressBar.InvokeRequired)
            {
                DisplayPLufferDataCallback display = new DisplayPLufferDataCallback(DisplayPLufferData);
                this.Invoke(display, data);
            }
            else
            {
                lufferprogressBar.Value = data + 10;
            }
        }
        private void DisplayPHeightData(Int16 data)
        {
            if (this.heightprogressBar.InvokeRequired)
            {
                DisplayPHeightDataCallback display = new DisplayPHeightDataCallback(DisplayPHeightData);
                this.Invoke(display, data);
            }
            else
            {
                heightprogressBar.Value = data + 10;
            }
        }
        private void DisplayControl(ControlData controldata)
        {
            DisplayRotationData(controldata.data.rotation);
            DisplayLufferData(controldata.data.luffer);
            DisplayHeightData(controldata.data.height);
            DisplayWindspeedData(controldata.data.windspeed);
            DisplayLeanData(controldata.data.lean);
            DisplayTorqueData(controldata.data.torque);
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
        private void Output(params object[] args)
        {
            console.Invoke(setListBoxCallback, args);
        }
        protected override void OnLoad(EventArgs e)
        {
            console.Invoke(setListBoxCallback, "onload");
        }
    }
}
