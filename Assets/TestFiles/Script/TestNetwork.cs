using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
public class TestNetwork : MonoBehaviour
{
    public Button connectBtn;
    public Button disconnectBtn;
    void Start()
    {
        connectBtn.onClick.AddListener(() =>
        {
            GNetworkManager.Connect("192.168.0.114", 8888);
        });

        disconnectBtn.onClick.AddListener(() =>
        {
            GNetworkManager.Close();
        });

        GNetworkManager.AddNetEventListener(NetEvent.ConnectSucc, (str) =>
        {
            GDebug.Instance.Log("服务器连接成功");
        });

        GNetworkManager.AddNetEventListener(NetEvent.Close, (str) =>
        {
            GDebug.Instance.Log("本机成功断开连接。");
        });
    }
}
