using NetMessage;
using System.Net.Sockets;

namespace ServerProgram
{
    class ClientState
    {
        public Socket socket;
        public ByteBuffer readBuffer = new ByteBuffer(1024);
        public long lastPingTime = 0;
    }
}
