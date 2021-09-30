using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

namespace Framework.GNetwork
{
    public static class GNetworkManager
    {
        //自定义类型
        public delegate void NetEventListener(string str);
        public delegate void MsgEventListener(BaseMsg msg);

        /// <summary>
        /// 当前客户端状态
        /// </summary>
        public static ClientConnectState clientState;
        /// <summary>
        /// 长度信息数字类型
        /// </summary>
        public static readonly LengthMsgDataType LMDT = LengthMsgDataType.INT16;

        private static Socket socket;
        private static ByteBuffer readBuffer;
        private static Queue<ByteBuffer> sendQueue;
        private static List<BaseMsg> msgList;
        private static int msgCount=0;
        private static readonly int MAX_MESSAGE_FIRE = 10;

        //心跳机制的变量
        public static bool isUseHeartBeat = true;
        private static float lastSendHeartBeatPingMsg = 0;
        private static float lastRecvHeartBeatPongMsg = 0;
        private static readonly int heartBeatInterval = 10;
        private static MsgHeartBeatPing PingMsg;

        //自定义的网络连接事件
        private static Dictionary<NetEvent, NetEventListener> netEventListeners = new Dictionary<NetEvent, NetEventListener>();
        /// <summary>
        /// 添加网络监听事件
        /// </summary>
        /// <param name="netEvent">事件类型</param>
        /// <param name="listener">监听事件</param>
        public static void AddNetEventListener(NetEvent netEvent, NetEventListener listener)
        {
            if (netEventListeners.ContainsKey(netEvent))
                netEventListeners[netEvent] += listener;
            else
                netEventListeners.Add(netEvent, listener);
        }

        /// <summary>
        /// 移除网络监听事件
        /// </summary>
        /// <param name="netEvent">事件类型</param>
        /// <param name="listener">监听事件</param>
        public static void RemoveNetEventListener(NetEvent netEvent, NetEventListener listener)
        {
            if (netEventListeners.ContainsKey(netEvent))
            {
                netEventListeners[netEvent] -= listener;
                if (netEventListeners[netEvent] == null)
                    netEventListeners.Remove(netEvent);
            }
        }

        /// <summary>
        /// 调用网络监听事件
        /// </summary>
        /// <param name="netEvent">调用类型</param>
        /// <param name="param">传进的参数</param>
        public static void CallNetEvents(NetEvent netEvent, string param = "")
        {
            if (netEventListeners.ContainsKey(netEvent))
                netEventListeners[netEvent]?.Invoke(param);
        }



        private static Dictionary<string, MsgEventListener> msgEventListeners = new Dictionary<string, MsgEventListener>();
        public static void AddMsgEventListener(ProtoName protoName, MsgEventListener listener)
        {
            string key = protoName.ToString();
            if (msgEventListeners.ContainsKey(key))
                msgEventListeners[key] += listener;
            else
                msgEventListeners.Add(key, listener);
        }

        public static void RemoveMsgEventListener(ProtoName protoName,MsgEventListener listener)
        {
            string key = protoName.ToString();
            if (msgEventListeners.ContainsKey(key))
            {
                msgEventListeners[key] -= listener;
                if (msgEventListeners[key] == null)
                    msgEventListeners.Remove(key);
            }
        }

        public static void CallMsgEvent(string typeName,BaseMsg msg)
        {
            if (typeName == "" || msg == default)
                return;
            if (msgEventListeners.ContainsKey(typeName))
                msgEventListeners[typeName]?.Invoke(msg);
        }





        public static void InitClientState()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            readBuffer = new ByteBuffer();
            sendQueue = new Queue<ByteBuffer>();
            msgList = new List<BaseMsg>();
            
            //初始化心跳机制的变量
            PingMsg = new MsgHeartBeatPing();
            lastSendHeartBeatPingMsg = Time.time;
            lastRecvHeartBeatPongMsg = Time.time;
            if (!msgEventListeners.ContainsKey(ProtoName.MsgHeartBeatPong.ToString()))
            {
                AddMsgEventListener(ProtoName.MsgHeartBeatPong, (str) =>
                {
                    lastRecvHeartBeatPongMsg = Time.time;
                });
            }

            clientState = ClientConnectState.Disconnect;
        }

        public static void Connect(string ip, int port)
        {
            if (socket != null && socket.Connected)
            {
                GDebug.Instance.Warn("当前的客户端已经连接上服务器,请不要尝试重新连接操作...");
                return;
            }
            else if (clientState == ClientConnectState.Connecting)
            {
                GDebug.Instance.Warn("客户端正在连接,请不要频繁响应连接操作...");
                return;
            }

            //初始化客户端状态
            InitClientState();
            clientState = ClientConnectState.Connecting;
            //异步连接
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                socket = ar.AsyncState as Socket;
                socket.EndConnect(ar);
                clientState = ClientConnectState.Connected;
                CallNetEvents(NetEvent.ConnectSucc);

                //开始接收来自客户端的信息
                socket.BeginReceive(readBuffer.buffer, readBuffer.writeIdx, readBuffer.RemainLength, 0, ReceiveCallback, socket);
            }
            catch (SocketException ex)
            {
                GDebug.Instance.Error($"连接服务器失败。以下为详细信息：\n{ex.ToString()}");
                CallNetEvents(NetEvent.ConnectFail, ex.ToString());
                clientState = ClientConnectState.Disconnect;
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar.AsyncState as Socket;
                int count = socket.EndReceive(ar);
                if(count==0)
                {
                    Close();
                    return;
                }

                readBuffer.writeIdx += count;
                //处理消息
                OnReceiveData();

                if(readBuffer.RemainLength<8)
                    readBuffer.ExpanCapcacityTwoTimes();

                socket.BeginReceive(readBuffer.buffer, readBuffer.writeIdx, readBuffer.RemainLength, 0, ReceiveCallback, socket);
            }
            catch(SocketException ex)
            {
                GDebug.Instance.Error($"从服务器接收消息发生了异常。以下为详细信息：\n{ex}");
            }
        }

        private static void OnReceiveData()
        {
            if(readBuffer.CanGetMsgLength())
            {
                if (!readBuffer.CanGetCompleteMsg())
                    return;


                int msg_length = readBuffer.GetMsgLengthFromBytes();
                int msg_symbol_length = readBuffer.GetMsgSymbolLengthFromeBytes();
                byte[] data = new byte[msg_length];
                int readCount;
                if (readBuffer.Read(data, 0, out readCount))
                {
                    string typeName;
                    //读取了完整的数据
                    BaseMsg msg = BaseMsg.ParseBytesPage(data, msg_symbol_length,out typeName);
                    lock(msgList)
                    {
                        msgList.Add(msg);
                    }
                    msgCount++;

                    if (readBuffer.CanGetMsgLength())
                        OnReceiveData();
                }
                else
                    return;
            }
        }

        public static void Send<T>(T msg) where T : BaseMsg
        {
            if (socket == null || !socket.Connected)
                return;
            if (clientState == ClientConnectState.Connecting||clientState==ClientConnectState.Closing)
                return;

            ByteBuffer package = BaseMsg.GetBytesPackage(msg.protoName,msg);
            int count = 0;
            lock(sendQueue)
            {
                sendQueue.Enqueue(package);
                count = sendQueue.Count;
            }

            if (count == 1)//立即处理
            {
                socket.BeginSend(package.buffer, 0, package.DataLength, 0, SendCallback, socket);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar.AsyncState as Socket;
                if (socket == null || !socket.Connected)
                    return;

                //结束发送
                int count = socket.EndSend(ar);
                ByteBuffer byteBuffer;
                lock (sendQueue)
                    byteBuffer = sendQueue.First();

                //完整发送
                byteBuffer.readIdx = count;
                if(byteBuffer.DataLength==0)
                {
                    lock(sendQueue)
                    {
                        sendQueue.Dequeue();
                        byteBuffer = sendQueue.First();
                    }
                }

                //若队列中仍存在信息需要发送
                if (byteBuffer != null)
                    socket.BeginSend(byteBuffer.buffer, byteBuffer.readIdx, byteBuffer.DataLength, 0, SendCallback, socket);
                else if (clientState == ClientConnectState.Closing)
                    socket.Close();
            }
            catch(SocketException ex)
            {
                GDebug.Instance.Error($"在发送消息至服务器的过程中发生了异常。以下为详细信息：\n{ex}");
            }
        }

        public static void Update()
        {
            MsgUpdate();
            HeartBeatUpdate();
        }

        public static void MsgUpdate()
        {
            if (msgCount == 0)
                return;

            for(int i=0;i<MAX_MESSAGE_FIRE;i++)
            {
                //获取第一条消息
                BaseMsg msg = null;
                lock(msgList)
                {
                    if(msgList.Count>0)
                    {
                        msg = msgList[0];
                        msgList.RemoveAt(0);
                        msgCount--;
                    }
                }

                if (msg != null)
                    CallMsgEvent(msg.protoName, msg);
                else
                    break;
            }
        }

        public static void HeartBeatUpdate()
        {
            if (!isUseHeartBeat) return;
            if (socket == null || !socket.Connected) return;
            
            if(Time.time-lastSendHeartBeatPingMsg>=heartBeatInterval)
            {
                Send(PingMsg);
                lastSendHeartBeatPingMsg = Time.time;
            }

            if (Time.time - lastRecvHeartBeatPongMsg > heartBeatInterval * 4)
                Close();
        }

        public static void Close()
        {
            if (socket == null || !socket.Connected)
                return;

            if (clientState == ClientConnectState.Connecting)
                return;

            if (sendQueue.Count > 0)
                clientState = ClientConnectState.Closing;
            else
            {
                socket.Close();
                CallNetEvents(NetEvent.Close);
            }
        }
    }
}
