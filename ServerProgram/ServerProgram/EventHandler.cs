using System;
using System.Collections.Generic;
using System.Text;

namespace ServerProgram
{
    class EventHandler
    {
        public static void OnDisconnect(ClientState c)
        {
            string desc = c.socket.RemoteEndPoint.ToString();
            string sendStr = $"Leave|{desc},";
            foreach (ClientState cs in Program.clients.Values)
                Program.Send(cs, sendStr);
        }
    }
}
