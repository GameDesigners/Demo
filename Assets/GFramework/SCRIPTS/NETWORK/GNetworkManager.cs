using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public enum ClientConnectState
{
    Disconnect,
    Connecting,
    Closing,
    Connected,
}

public enum NetEvent
{
    ConnectSucc,
    ConnectFail,
    Close
}

public static class GNetworkManager
{
    //自定义类型
    public delegate void EventListener(string str);

    public static ClientConnectState clientState;
    private static Socket socket;
    private static ByteBuffer readBuffer;
    private static Queue<ByteBuffer> sendQueue;
    
    //自定义的网络连接事件
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
    
    /// <summary>
    /// 添加网络监听事件
    /// </summary>
    /// <param name="netEvent">事件类型</param>
    /// <param name="listener">监听事件</param>
    public static void AddNetEventListener(NetEvent netEvent,EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
            eventListeners[netEvent] += listener;
        else
            eventListeners.Add(netEvent, listener);
    }

    /// <summary>
    /// 移除网络监听事件
    /// </summary>
    /// <param name="netEvent">事件类型</param>
    /// <param name="listener">监听事件</param>
    public static void RemoveNetEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
            if (eventListeners[netEvent] == null)
                eventListeners.Remove(netEvent);
        }
    }

    /// <summary>
    /// 调用网络监听事件
    /// </summary>
    /// <param name="netEvent">调用类型</param>
    /// <param name="param">传进的参数</param>
    public static void CallNetEvents(NetEvent netEvent,string param="")
    {
        if (eventListeners.ContainsKey(netEvent))
            eventListeners[netEvent]?.Invoke(param);
    }


    
    
    
    
    public static void InitClientState()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuffer = new ByteBuffer();
        sendQueue = new Queue<ByteBuffer>();
        clientState = ClientConnectState.Disconnect;
    }
    
    public static void Connect(string ip,int port)
    {
        if (socket != null && socket.Connected)
        {
            GDebug.Instance.Warn("当前的客户端已经连接上服务器,请不要尝试重新连接操作...");
            return;
        }
        else if (clientState==ClientConnectState.Connecting)
        {
            GDebug.Instance.Warn("客户端正在连接,请不要频繁响应连接操作...");
            return;
        }

        //初始化客户端状态
        InitClientState();
        clientState = ClientConnectState.Connecting;
        //异步连接
        socket.BeginConnect(ip, port, ConnectCallback,socket);
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            socket = ar.AsyncState as Socket;
            socket.EndConnect(ar);
            clientState = ClientConnectState.Connected;
            CallNetEvents(NetEvent.ConnectSucc);
        }
        catch(SocketException ex)
        {
            GDebug.Instance.Error($"连接服务器失败。以下为详细信息：\n{ex.ToString()}");
            CallNetEvents(NetEvent.ConnectFail, ex.ToString());
            clientState = ClientConnectState.Disconnect;
        }
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
