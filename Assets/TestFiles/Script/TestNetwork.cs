using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Framework.GNetwork;
using System.Net;
using System.Linq;

public class TestNetwork : MonoBehaviour
{
    public Button connectBtn;
    public Button disconnectBtn;
    void Start()
    {
        connectBtn.onClick.AddListener(() =>
        {
            GNetworkManager.Connect(GetIp(), 8888);
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

    public void Update()
    {
        GNetworkManager.Update();
    }

    public string GetIp() => Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(p => p.AddressFamily.ToString() == "InterNetwork")?.ToString();
}
