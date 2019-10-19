using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SiMay.Sockets;
using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Server;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;

namespace SiMaySocketTestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TcpSocketSaeaClientAgent client;
        TcpSocketSaeaServer server;
        private void Form1_Load(object sender, EventArgs e)
        {
            //模式说明
            //Full模式:仅接收发送数据（高性能的数据接收模型，可启用TcpKeepAlive选项）注：数据压缩丶AppKeepAlive选项在Full模式下启用无效
            //Pack模式:自动处理分包（高性能的数据接收模型，自动处理分包，可启用数据压缩丶应用层心跳包丶TcpKeepAlive选项）


            //设置在Socket初始化后无法修改
            TcpSocketSaeaClientConfiguration clientConfig = new TcpSocketSaeaClientConfiguration();
            clientConfig.KeepAlive = false;
            clientConfig.AppKeepAlive = true;
            client = TcpSocketsFactory.CreateClientAgent(TcpSocketSaeaSessionType.Packet, clientConfig, Client_CompletetionNotify);



            TcpSocketSaeaServerConfiguration serverConfig = new TcpSocketSaeaServerConfiguration();
            serverConfig.KeepAlive = false;
            serverConfig.AppKeepAlive = true;
            server = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Packet, serverConfig, Server_CompletetionNotify);
        }

        private void Server_CompletetionNotify(TcpSocketCompletionNotify e, TcpSocketSaeaSession session)
        {
            this.Invoke(new Action(() =>
            {
                switch (e)
                {
                    case TcpSocketCompletionNotify.OnConnected:

                        //session初始化完成连接的事件
                        //调用session.SendAsync();对session发送消息
                        //调用session.Close(true)断开session连接，并通知断开事件
                        //可在session.AppTokens中绑定应用对象，以便在其他异步事件中调用

                        this.listBox1.Items.Add("successful client connection");

                        break;
                    case TcpSocketCompletionNotify.OnSend:

                        //session发送数据通知事件

                        //session.SendTransferredBytes == 以发送数据长度

                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:

                        //session数据接收通知事件

                        //session.ReceiveBytesTransferred == 本次接收数据长度
                        //Packet模式下 session.CompletedBuffer.Length == 完整数据包长度

                        //this.listBox1.Items.Add("len:" + session.CompletedBuffer.Length + " msg:" + Encoding.UTF8.GetString(session.CompletedBuffer, 0, session.ReceiveBytesTransferred));


                        break;
                    case TcpSocketCompletionNotify.OnDataReceived:
                        if (session.CompletedBuffer.Length != 15)
                            throw new Exception();


                        session.SendAsync(session.CompletedBuffer);


                        //Packet模式session自动处理分包的完成事件

                        //var pack = session.CompletedBuffer;//完整数据包
                        break;
                    case TcpSocketCompletionNotify.OnClosed:

                        //session断开通知事件

                        this.listBox1.Items.Add("server :client offline");
                        break;
                }
            }));


        }

        private void Client_CompletetionNotify(TcpSocketCompletionNotify e, TcpSocketSaeaSession session)
        {
            this.Invoke(new Action(() =>
            {
                switch (e)
                {
                    case TcpSocketCompletionNotify.OnConnected:
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        //this.listBox1.Items.Add("len:" + session.CompletedBuffer.Length + " msg:" + Encoding.UTF8.GetString(session.CompletedBuffer, 0, session.ReceiveBytesTransferred));
                        break;
                    case TcpSocketCompletionNotify.OnDataReceived:
                        if (session.CompletedBuffer.Length != 15)
                            throw new Exception();
                        Console.WriteLine(session.CompletedBuffer.Length + ":" + Encoding.UTF8.GetString(session.CompletedBuffer, 0, session.CompletedBuffer.Length));
                        session.SendAsync(session.CompletedBuffer);
                        //this.listBox1.Items.Add(session.CompletedBuffer.Length);
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
                        var xx = session.AppTokens;
                        this.listBox1.Items.Add("client disconnect");
                        break;
                }
            }));

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //对服务器发起连接
            client.ConnectToServer(new IPEndPoint(IPAddress.Parse("94.191.115.121"), 5211));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //断开所有连接，并通知断开事件
            client.DisconnectAll(true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //对所有session广播数据
            client.BroadcastAsync(Encoding.UTF8.GetBytes("服务器你好啊"));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //启动监听
            server.Listen(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 5211));
            this.listBox1.Items.Add("listening...");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            server.BroadcastAsync(Encoding.UTF8.GetBytes("你好鸭client"));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            server.DisconnectAll(true);
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
