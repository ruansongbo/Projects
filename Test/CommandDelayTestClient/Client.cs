using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Globalization;
using System.IO;
using System.Text;

namespace CommandDelayTestClient
{
    public partial class Client : Form
    {
        private IPAddress serverIP;                             //服务端IP地址
        private int serverPort;                                 //服务端监听端口
        private IPEndPoint serverIPEndPoint;                    //服务端地址端口
        private Socket clientSocket;                            //客户端套接字
        static ManualResetEvent lockSocket = new ManualResetEvent(false);//实例化套接字锁
        byte[] receiveByte = new byte[1024];                    //异步等待锁
        private delegate void SetListBoxCallback(string str);
        private SetListBoxCallback setListBoxCallback;
        public FileStream fs = new FileStream("D:\\test.txt", FileMode.Create);
        public Client()
        {
            InitializeComponent();
            setListBoxCallback = new SetListBoxCallback(SetListBox);


        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString("HH点mm分ss秒fff毫秒");//显示当前时间
        }

        private void bConnect_Click(object sender, EventArgs e)
        {
            serverIP = IPAddress.Parse(tBIP.Text);              //服务端IP地址
            serverPort = Int32.Parse(tBPort.Text);              //服务端监听端口
            serverIPEndPoint = new IPEndPoint(serverIP, serverPort);//实例化地址端口类
            //实例化一个TCP流式套接字
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //异步连接服务器
            clientSocket.BeginConnect(serverIPEndPoint, new AsyncCallback(ConnectCallback), clientSocket);
            lockSocket.WaitOne();                               //阻止当前线程，等待连接成功
        }
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            Socket client = (Socket)asyncResult.AsyncState;     //客户端套接字
            client.EndConnect(asyncResult);                     //结束连接请求
            Thread thread = new Thread(new ThreadStart(ReceiveThread));//实例化接收线程
            thread.Start();                                     //启动接收线程
            lockSocket.Set();                                   //释放异步等待锁
        }
        private void ReceiveThread()
        {
            try
            {
                //开始异步接收服务端消息
                clientSocket.BeginReceive(receiveByte, 0, receiveByte.Length, 0,
                    new AsyncCallback(AsyncReceiveCall), clientSocket);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        private void AsyncReceiveCall(IAsyncResult asyncResult)
        {
            //接收异步读取，并返回接收到消息的字节数
            int bytesRead = clientSocket.EndReceive(asyncResult);
            //将缓存中的消息转换成字符串
            string receiveString = Encoding.Default.GetString(receiveByte, 0, bytesRead);
            DateTime dt = DateTime.Parse(receiveString);
            label2.Invoke(setListBoxCallback, dt.ToString("HH点mm分ss秒fff毫秒"));
            TimeSpan span = dt.Subtract(DateTime.Now);
            int count = span.ToString().LastIndexOf(':');
            string str = span.ToString().Substring(count+1,10)+',';
            byte[] data = System.Text.Encoding.Default.GetBytes(str);
            
            fs.Write(data, 0, str.Length);
            //开始新一轮的异步接收
            clientSocket.BeginReceive(receiveByte, 0, receiveByte.Length, 0,
                new AsyncCallback(AsyncReceiveCall), clientSocket);
        }
        private void SetListBox(string str)
        {
            label2.Text = str;//显示接收时间
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fs.Flush();
            fs.Close();
        }
    }
}
