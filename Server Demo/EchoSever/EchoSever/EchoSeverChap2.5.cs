using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace EchoSeverChap2
{

    class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];
    }

    class MainClass
    {
        // 监听 Socket
        static Socket listenfd;
        // 客户端Socket及状态信息
        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Socket
            Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(iPEp);

            // Listen
            listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");
            // 主循环
            while(true)
            {
                // 检查listenfd
                if(listenfd.Poll(0,SelectMode.SelectRead))
                {
                    ReadListenfd(listenfd);
                }
                // 检查clientfd
                foreach(ClientState s in clients.Values)
                {
                    Socket clientfd = s.socket;
                    if(clientfd.Poll(0,SelectMode.SelectRead))
                    {
                        if(!ReadClientfd(clientfd))
                        {
                            break;
                        }
                    }
                }
                // 防止CPU占用过高
                System.Threading.Thread.Sleep(1);
            }

        }
        // 读取Listenfd
        public static void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }
        //读取Clientfd
        public static bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            // 接收
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuff);
            }
            catch(SocketException ex)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Receive SocketException" + ex.ToString());
                return false;
            }
            // 广播
            string sendStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            Console.WriteLine("Receive" + recvStr);
            string sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        }

        // Accept回调
        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("[服务器]Accept");
                Socket listenfd = (Socket)ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);

                // clients列表
                ClientState state = new ClientState();
                state.socket = clientfd;
                clients.Add(clientfd, state);

                // 接收数据BeginReceive
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);

                // 继续Accept
                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Socket Accept fail" + ex.ToString());
            }
        }

        // Receive 回调
        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ClientState state = (ClientState)ar.AsyncState;
                Socket clientfd = state.socket;
                int count = clientfd.EndReceive(ar);

                // 客户端关闭
                if(count == 0)
                {
                    clientfd.Close();
                    clients.Remove(clientfd);
                    Console.WriteLine("Socket Close");
                    return;
                }

                string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes("echo" + recvStr);
                clientfd.Send(sendBytes);   // 减少代码量，不用异步
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Socket Receive fail" + ex.ToString());
            }
        }
    }



}
