using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Linq;
using NetMessage;
namespace ServerProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerNetManager.StartLoop(8888);
        }
    }
}
