using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace EchoSever
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
            // Accept
            listenfd.BeginAccept(AcceptCallback, listenfd);
            // 等待
            Console.ReadLine();

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
