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
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            foreach (ClientState cs in Program.clients.Values)
                cs.socket.Send(sendBytes);
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

            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            foreach (ClientState cs in Program.clients.Values)
                cs.socket.Send(sendBytes);
        }
    }
}
