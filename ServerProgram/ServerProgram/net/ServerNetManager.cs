using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace ServerProgram
{
    class ServerNetManager
    {
        /// <summary>
        /// 监听Socket
        /// </summary>
        public static Socket listenfd;

        /// <summary>
        /// 客户端Socket及其状态信息
        /// </summary>
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();


        public static int pingInterval = 10;

        /// <summary>
        /// 服务器向客户端发送消息的发送队列
        /// </summary>
        private static Queue<ByteBuffer> sendQueue = new Queue<ByteBuffer>();

        /// <summary>
        /// 开启服务器某个端口的监听循环
        /// </summary>
        /// <param name="listenPort"></param>
        public static void StartLoop(int listenPort)
        {
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(GetIp());
            IPEndPoint ipEp = new IPEndPoint(ipAdr, listenPort);
            listenfd.Bind(ipEp);
            listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");

            List<Socket> checkRead = new List<Socket>();
            while (true)
            {
                checkRead.Clear();
                checkRead.Add(listenfd);
                foreach (ClientState s in clients.Values)
                    checkRead.Add(s.socket);

                Socket.Select(checkRead, null, null, 1000);
                foreach (Socket s in checkRead)
                {
                    if (s == listenfd)
                        ReadListenerfd(s);
                    else
                        ReadClientfd(s);
                }
                Timer();
            }
        }

        /// <summary>
        /// 接收客户端的连接请求
        /// </summary>
        /// <param name="listenfd"></param>
        public static void ReadListenerfd(Socket listenfd)
        {
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            state.lastPingTime = Utils.GetTimerStamp();
            clients.Add(clientfd, state);
            Console.WriteLine($"CLIENT [{clientfd.RemoteEndPoint}] Socket Connect");
        }

        public static bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            ByteBuffer readBuffer = state.readBuffer;
            if (readBuffer.RemainLength <= 0)
            {
                OnReceiveData(state);
                readBuffer.RevertBuffer();
            }

            if (readBuffer.RemainLength <= 0)
            {
                Console.WriteLine("接受错误，目前接受的指令已经超过了1024.");
                Close(state);
                return false;
            }

            int count = 0;
            try
            {
                count = clientfd.Receive(readBuffer.buffer, readBuffer.writeIdx, readBuffer.RemainLength, 0);
                //Console.WriteLine($"本次接收到的数据为：{count}bytes");
            }
            catch (SocketException ex)
            {
                Close(state);
                Console.WriteLine("Receive SocketException" + ex.ToString());
                return false;
            }

            if (count <= 0)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] ob = { state };
                mei.Invoke(null, ob);

                Console.WriteLine($"CLIENT [{clientfd.RemoteEndPoint}] Socket Close");
                clientfd.Close();
                clients.Remove(clientfd);
                return false;
            }

            readBuffer.writeIdx += count;
            OnReceiveData(state);
            return true;
        }

        public static void Send<T>(ClientState cs, T msg) where T : BaseMsg
        {
            if (cs == null)
                return;

            if (!cs.socket.Connected)
                return;

            ByteBuffer package = BaseMsg.GetBytesPackage(msg.protoName, msg);
            //Console.WriteLine($"Send   [socket:{cs.socket.RemoteEndPoint}]  [msg:{msg.protoName}]  [size:{package.DataLength}]");
            int count = 0;
            lock (sendQueue)
            {
                sendQueue.Enqueue(package);
                count = sendQueue.Count;
            }

            if (count == 1)//立即处理
            {
                cs.socket.BeginSend(package.buffer, 0, package.DataLength, 0, SendCallback, cs.socket);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar.AsyncState as Socket;
                if (socket == null)
                    return;

                if (!socket.Connected)
                    return;

                //结束发送
                int count = socket.EndSend(ar);
                ByteBuffer byteBuffer;
                lock (sendQueue)
                    byteBuffer = sendQueue.First();

                //完整发送
                byteBuffer.readIdx = count;
                if (byteBuffer.DataLength == 0)
                {
                    lock (sendQueue)
                    {
                        sendQueue.Dequeue();
                        if (sendQueue.Count > 0)
                            byteBuffer = sendQueue.First();
                    }
                }

                //若队列中仍存在信息需要发送
                if (byteBuffer != null && byteBuffer.DataLength != 0)
                    socket.BeginSend(byteBuffer.buffer, byteBuffer.readIdx, byteBuffer.DataLength, 0, SendCallback, socket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"在发送消息至服务器的过程中发生了异常。以下为详细信息：\n{ex}");
            }
        }

        /// <summary>
        /// 接受的消息处理
        /// </summary>
        public static void OnReceiveData(ClientState state)
        {
            ByteBuffer readBuffer = state.readBuffer;
            if (readBuffer.CanGetMsgLength())
            {
                if (!readBuffer.CanGetCompleteMsg())
                    return;

                int msg_length = readBuffer.GetMsgLengthFromBytes();
                int msg_symbol_length = readBuffer.GetMsgSymbolLengthFromeBytes();
                byte[] data = new byte[msg_length - (int)NetMessage.Core.LMDT];
                int readCount;
                if (readBuffer.Read(data, 0, out readCount))
                {
                    string protoName;
                    BaseMsg msg = BaseMsg.ParseBytesPage(data, msg_symbol_length, out protoName);
                    //Console.WriteLine($"protoName:{protoName}");
                    MethodInfo mei = typeof(MsgHandler).GetMethod(protoName);
                    object[] ob = { state, msg };
                    mei.Invoke(null, ob);

                    if (readBuffer.CanGetMsgLength())
                        OnReceiveData(state);
                }
            }
        }

        /// <summary>
        /// 关闭客户段
        /// </summary>
        /// <param name="state"></param>
        public static void Close(ClientState state)
        {
            MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
            object[] ob = { state };
            mei.Invoke(null, ob);
            //关闭
            state.socket.Close();
            clients.Remove(state.socket);
        }

        public static void Timer()
        {
            MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
            object[] ob = { };
            mei.Invoke(null, ob);
        }

        /// <summary>
        /// 获取服务器主机的默认IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIp() => Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(p => p.AddressFamily.ToString() == "InterNetwork")?.ToString();
    }
}
