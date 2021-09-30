using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Linq;
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
            IPAddress ipAdr = IPAddress.Parse(GetIp());
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

        public static string GetIp() => Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(p => p.AddressFamily.ToString() == "InterNetwork")?.ToString();

        /// <summary>
        /// 接收客户端的连接请求
        /// </summary>
        /// <param name="listenfd"></param>
        public static void ReadListenerfd(Socket listenfd)
        {
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
            Console.WriteLine($"CLIENT [{clientfd.RemoteEndPoint}] Socket Connect");
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

                Console.WriteLine($"CLIENT [{clientfd.RemoteEndPoint}] Socket Close");
                clientfd.Close();
                clients.Remove(clientfd);
                return false;
            }

            //广播
            //string recvStr = System.Text.Encoding.Default.GetString(state.readBuffer, 0, count);
            byte[] msgLengthBytes = new byte[2];
            Array.Copy(state.readBuffer,0, msgLengthBytes,0,2);
            int msg_length = BitConverter.ToUInt16(msgLengthBytes);

            byte[] symbolLengthBytes = new byte[2];
            Array.Copy(state.readBuffer, 2, symbolLengthBytes, 0, 2);
            int symbol_length = BitConverter.ToUInt16(symbolLengthBytes);

            byte[] jsonBytes = new byte[msg_length - 2];
            Array.Copy(state.readBuffer, 4, jsonBytes, 0, msg_length - 2);
            string json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            Console.WriteLine($"Receive-->[msg_length:{msg_length}]  [symbol_length:{symbol_length}]  {json}");

            //string[] split = recvStr.Split('|');
            //string msgName = split[0];
            //string msgArgs = split[1];
            //string funName = "Msg" + msgName;
            //MethodInfo mi = typeof(MsgHandler).GetMethod(funName);
            //object[] o = { state, msgArgs };
            //mi.Invoke(null, o);
            return true;
        }

        public static void Send(ClientState cs,string sendStr)
        {
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            cs.socket.Send(sendBytes);
        }
    }
}
