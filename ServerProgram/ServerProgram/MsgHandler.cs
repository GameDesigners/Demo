using System;
using System.Collections.Generic;
using System.Text;

namespace ServerProgram
{
    class MsgHandler
    {
        public static void MsgEnter(ClientState c,string msgArgs)
        {
            Console.WriteLine($"[MsgEnter Enter Args] { msgArgs}");
            string[] split = msgArgs.Split(',');
            string desc = split[0];
            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);
            float eulY = float.Parse(split[4]);
            int hp = int.Parse(split[5]);
            //赋值
            c.x = x;
            c.y = y;
            c.z = z;
            c.eulY = eulY;
            c.hp = hp;

            //广播
            string sendStr = $"Enter|{msgArgs}";

            foreach (ClientState cs in Program.clients.Values)
                Program.Send(cs, sendStr);
        }

        public static void MsgMove(ClientState c,string msgArg)
        {
            string[] split = msgArg.Split(',');

            //解析参数
            string desc = split[0];
            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);

            //服务器端数据赋值
            c.x = x;
            c.y = y;
            c.z = z;

            //广播
            string sendStr = $"Move|{msgArg}";
            foreach (ClientState cs in Program.clients.Values)
                Program.Send(cs, sendStr);
        }

        public static void MsgList(ClientState c,string msgArgs)
        {
            string sendStr = "List|";
            
            foreach(ClientState cs in Program.clients.Values)
            {
                sendStr += cs.socket.RemoteEndPoint.ToString() + ",";
                sendStr += cs.x.ToString() + ",";
                sendStr += cs.y.ToString() + ",";
                sendStr += cs.z.ToString() + ",";
                sendStr += cs.eulY.ToString() + ",";
                sendStr += cs.hp.ToString() + ",";
            }

            foreach (ClientState cs in Program.clients.Values)
                Program.Send(cs, sendStr);
        }

        public static void MsgAttack(ClientState c,string msgArgs)
        {
            //广播
            string sendStr = $"Attack|{msgArgs}";
            foreach(ClientState cs in Program.clients.Values)
                Program.Send(cs, sendStr);
        }
    }
}
