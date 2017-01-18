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
        private UInt16 status;
        private RemoteData remotedata;
        private ControlData controldata;
        private TcpClient client;
        private IPAddress serverIP = IPAddress.Parse("172.31.103.2");
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

        //*************SDK*********
        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private Int32 m_lRealHandle = -1;
        private string str;
        //&&&&&&&&&&&&&&SDK&&&&&&&&&&&&&&
        public FormRemote()
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

        private void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                GetRemoteData();
                remotedata.data.status = status;
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
            status = 1;
            aTimer.Enabled = true;
        }

        private void stopbutton_Click(object sender, EventArgs e)
        {
            aTimer.Enabled = false;
            status = 0;
            try
            {
                GetRemoteData();
                remotedata.encode();
                networkStream.BeginWrite(remotedata.databuffer, 0, remotedata.length, new AsyncCallback(SendCallback), networkStream);
                networkStream.Flush();
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



 
