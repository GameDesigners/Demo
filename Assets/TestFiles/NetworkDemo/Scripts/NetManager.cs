using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    /// <summary>
    /// 套接字
    /// </summary>
    private static Socket socket;

    /// <summary>
    /// 接收缓冲区
    /// </summary>
    private static byte[] readBuffer=new byte[1024];

    /// <summary>
    /// 监听事件委托类型
    /// </summary>
    /// <param name="str"></param>
    public delegate void MyListner(string str);

    /// <summary>
    /// 事件列表
    /// </summary>
    private static Dictionary<string, MyListner> listeners = new Dictionary<string, MyListner>();

    /// <summary>
    /// 消息列表
    /// </summary>
    private static List<string> msgList = new List<string>();

    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="msgName">信息名称</param>
    /// <param name="listener">监听事件</param>
    public static void AddListener(string msgName,MyListner listener)
    {
        listeners[msgName] = listener;
    }

    /// <summary>
    /// 获取客户端描述
    /// </summary>
    /// <returns></returns>
    public static string GetDesc()
    {
        if (socket == null)
            return "";
        if (!socket.Connected)
            return "";
        return socket.LocalEndPoint.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public static void Connect(string ip,int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ip, port);
        socket.BeginReceive(readBuffer, 0, 1024, 0, ReceiveCallback, socket);
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            string resultStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);
            msgList.Add(resultStr);
            socket.BeginReceive(readBuffer, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            GDebug.Instance.Error($"Socket接收失败！\n{ex.ToString()}");
        }
    }

    public static void Send(string sendStr)
    {
        if (socket == null)
            return;
        if (!socket.Connected)
            return;

        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
    }

    public static void Update()
    {
        if (msgList.Count <= 0)
            return;

        string msgStr = msgList[0];
        msgList.RemoveAt(0);
        string[] split = msgStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];
        if (listeners.ContainsKey(msgName))
            listeners[msgName](msgArgs);
    }
    
}
