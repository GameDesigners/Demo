using System;
using System.Collections.Generic;
using System.Text;

namespace ServerProgram
{
    class EventHandler
    {
        public static void OnDisconnect(ClientState c)
        {
            Console.WriteLine($"CLIENT [{c.socket.RemoteEndPoint}] Socket Close");
        }

        public static void OnTimer()
        {
            CheckPing();
        }

        private static void CheckPing()
        {
            long currentTime = Utils.GetTimerStamp();
            foreach (var cs in ServerNetManager.clients.Values)
            {
                if(currentTime - cs.lastPingTime>ServerNetManager.pingInterval*2)
                {
                    Console.WriteLine($"Error  [socket:{cs.socket.RemoteEndPoint}]  长时间没有获得此客户端Ping信号...");
                    ServerNetManager.Close(cs);
                    return;
                }
            }
        }
    }
}
