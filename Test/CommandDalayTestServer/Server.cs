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

namespace CommandDalayTestServer
{
    public partial class Server : Form
    {
        System.Timers.Timer aTimer = new System.Timers.Timer();
        private IPEndPoint serverIPEndPoint;                    //IP地址和端口
        private int listenPort;                                 //监听端口
        private Socket listenSocket;                            //监听套接字
        private Socket clientSocket;                            //客户端套接字
        private Thread threadAccept;                            //等待客户端连接线程
        private Thread threadReceive;                           //等待接收客户端消息线程
        private byte[] receiveByte = new byte[1024];            //接收消息缓存
        private static ManualResetEvent lockSocket = new ManualResetEvent(false);//异步等待锁
        public Server()
        {
            InitializeComponent();
            aTimer.Elapsed += new ElapsedEventHandler(aTimer_Elapsed);
            aTimer.Interval = 500;  //设置时间间隔 
            aTimer.Enabled = false;
        }
        private void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (clientSocket != null && clientSocket.Connected)//判断是否已经与客户端建立连接
            {
                //将需要发送的字符串转换为Byte数组
                Byte[] sendByte = Encoding.Default.GetBytes(DateTime.Now.ToString("HH点mm分ss秒fff毫秒"));
                //向客户端异步发送消息
                clientSocket.BeginSend(sendByte, 0, sendByte.Length, 0,
                    new AsyncCallback(SendCallback), clientSocket);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString("HH点mm分ss秒fff毫秒");//显示当前时间
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listenPort = 8002;              //输入监听端口号
            serverIPEndPoint = new IPEndPoint(IPAddress.Any, listenPort);//实例化地址端口类
            //实例化一个TCP流式套接字
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(serverIPEndPoint);                //将套接字绑定到本地端口
            listenSocket.Listen(50);                            //将套接字设置为侦听状态
            threadAccept = new Thread(new ThreadStart(AcceptThread));//等待客户端连接线程
            threadAccept.Start();                               //启动接受连接线程
        }
        private void AcceptThread()
        {
            while (true)
            {
                lockSocket.Reset();                             //设置异步等待锁为阻塞状态
                //异步等待客户端连接
                listenSocket.BeginAccept(new AsyncCallback(AcceptCall), listenSocket);
                lockSocket.WaitOne();                           //阻止当前线程，异步等待客户端请求连接
            }
        }
        private void AcceptCall(IAsyncResult asyncResult)
        {
            lockSocket.Set();                                   //释放异步等待锁
            //终止异步等待，并建立新的套接字与客户端进行通信
            clientSocket = listenSocket.EndAccept(asyncResult);
        }
        private void SendCallback(IAsyncResult asyncResult)
        {
            Socket socket = (Socket)asyncResult.AsyncState;
            int bytesSent = socket.EndSend(asyncResult);//结束挂起的异步发送，并返回发送的字节数
        }

        private void button2_Click(object sender, EventArgs e)
        {
            aTimer.Enabled = true;
        }
    }
}
