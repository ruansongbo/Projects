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

        //*************SDK*********
        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private Int32 m_lRealHandle = -1;
        private string str;
        //&&&&&&&&&&&&&&SDK&&&&&&&&&&&&&&

        private void ToExit()
        {
            isExit = true;
        }
        public FormServer()
        {
            InitializeComponent();            
            //****************SDK*********
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }
            //&&&&&&&&&&&&&&SDK&&&&&&&&&&&&&&
            setListBoxCallback = new SetListBoxCallback(SetListBox);
            exit = new Exit(ToExit);
        }

        //****************SDK*********
        protected override void Dispose(bool disposing)
        {
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            }
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
            }
            if (m_bInitSDK == true)
            {
                CHCNetSDK.NET_DVR_Cleanup();
            }
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        private void btnLogin_Click(object sender, System.EventArgs e)
        {
            if (textBoxIP.Text == "" || textBoxPort.Text == "" ||
                textBoxUserName.Text == "" || textBoxPassword.Text == "")
            {
                MessageBox.Show("Please input IP, Port, User name and Password!");
                return;
            }
            if (m_lUserID < 0)
            {
                string DVRIPAddress = textBoxIP.Text; //设备IP地址或者域名
                Int16 DVRPortNumber = Int16.Parse(textBoxPort.Text);//设备服务端口号
                string DVRUserName = textBoxUserName.Text;//设备登录用户名
                string DVRPassword = textBoxPassword.Text;//设备登录密码

                CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                //登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    //登录成功
                    MessageBox.Show("Login Success!");
                    btnLogin.Text = "退出";
                }

            }
            else
            {
                //注销登录 Logout the device
                if (m_lRealHandle >= 0)
                {
                    MessageBox.Show("Please stop live view firstly");
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lUserID = -1;
                btnLogin.Text = "登录";
            }
            return;
        }

        private void btnPreview_Click(object sender, System.EventArgs e)
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;//预览窗口
                lpPreviewInfo.lChannel = 1;//预te览的设备通道
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 15; //播放库播放缓冲区最大缓冲帧数

                CHCNetSDK.REALDATACALLBACK RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数
                IntPtr pUser = new IntPtr();//用户数据

                //打开预览 Start live view 
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    //预览成功
                    btnPreview.Text = "停止预览";
                }
            }
            else
            {
                //停止预览 Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lRealHandle = -1;
                btnPreview.Text = "开始预览";

            }
            return;
        }

        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, ref byte pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
        }
        private void btnJPEG_Click(object sender, EventArgs e)
        {
            string sJpegPicFileName;
            //图片保存路径和文件名 the path and file name to save
            sJpegPicFileName = "JPEG_test.jpg";

            int lChannel = 1; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图 Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                MessageBox.Show(str);
                return;
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName;
                MessageBox.Show(str);
            }
            return;
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            //录像保存路径和文件名 the path and file name to save
            string sVideoFileName;
            sVideoFileName = "Record_test.mp4";

            if (m_bRecord == false)
            {
                //强制I帧 Make a I frame
                int lChannel = 1; //通道号 Channel number
                CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, lChannel);

                //开始录像 Start recording
                if (!CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandle, sVideoFileName))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_SaveRealData failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    btnRecord.Text = "停止录像";
                    m_bRecord = true;
                }
            }
            else
            {
                //停止录像 Stop recording
                if (!CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopSaveRealData failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    str = "Successful to stop recording and the saved file is " + sVideoFileName;
                    MessageBox.Show(str);
                    btnRecord.Text = "开始录像";
                    m_bRecord = false;
                }
            }

            return;
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            //停止预览 Stop live view 
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                m_lRealHandle = -1;
            }

            //注销登录 Logout the device
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }
        //&&&&&&&&&&&&&&SDK&&&&&&&&&&&&&&

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
                Server2Control control = new Server2Control(client, Output, DisplayControl, ErrorHandle);
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
        private void ErrorHandle(Server2Control server2control)
        {
            controllist[server2control.controlID] = null;
            server2control.client.Close();
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
