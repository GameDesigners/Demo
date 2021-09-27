using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace ServerProgram
{

    class ClientState
    {
        public Socket socket;
        public byte[] readBuffer = new byte[1024];

        public int hp = -100;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float eulY = 0;
    }

    class Program
    {
        /// <summary>
        /// 监听Socket
        /// </summary>
        static Socket listenfd;

        /// <summary>
        /// 客户端Socket及其状态信息
        /// </summary>
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        static void Main(string[] args)
        {
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse("192.168.3.42");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");

            List<Socket> checkRead = new List<Socket>();
            while(true)
            {
                checkRead.Clear();
                checkRead.Add(listenfd);
                foreach(ClientState s in clients.Values)
                    checkRead.Add(s.socket);

                Socket.Select(checkRead, null, null, 1000);
                foreach(Socket s in checkRead)
                {
                    if (s == listenfd)
                        ReadListenerfd(s);
                    else
                        ReadClientfd(s);
                }
            }
        }

        /// <summary>
        /// 接收客户端的连接请求
        /// </summary>
        /// <param name="listenfd"></param>
        public static void ReadListenerfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }

        public static bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];

            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuffer);
            }
            catch(SocketException ex)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] ob = { state };
                mei.Invoke(null, ob);

                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Receive SocketException" + ex.ToString());
                return false;
            }

            if(count<=0)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] ob = { state };
                mei.Invoke(null, ob);

                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Socket Close");
                return false;
            }

            //广播
            string recvStr = System.Text.Encoding.Default.GetString(state.readBuffer, 0, count);
            Console.WriteLine("Receive-->" + recvStr);
            string[] split = recvStr.Split('|');
            string msgName = split[0];
            string msgArgs = split[1];
            string funName = "Msg" + msgName;
            MethodInfo mi = typeof(MsgHandler).GetMethod(funName);
            object[] o = { state, msgArgs };
            mi.Invoke(null, o);
            return true;
        }

        public static void Send(ClientState cs,string sendStr)
        {
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            cs.socket.Send(sendBytes);
        }
    }
}
