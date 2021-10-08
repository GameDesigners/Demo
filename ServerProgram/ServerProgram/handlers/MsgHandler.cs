using System;
using NetMessage;

namespace ServerProgram
{
    class MsgHandler
    {
        public static void MsgHeartBeatPing(ClientState c,BaseMsg msg)
        {
            Console.WriteLine($"Receive[socket:{c.socket.RemoteEndPoint}]  [msg:{msg.protoName}]");
            c.lastPingTime = Utils.GetTimerStamp();
            ServerNetManager.Send(c, new MsgHeartBeatPong());
        }

        public static void MsgHeartBeatPong(ClientState c, BaseMsg msg)
        {

        }
    }
}
