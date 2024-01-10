using System.Net;
using System.Text;

namespace ChatUygulamasi.ChatShared
{
    public static class ChatConstants
    {
        public static readonly int ServerPort = 19000;

        public static readonly IPAddress MulticastIP = IPAddress.Parse("224.168.9.55");

        public static readonly int MulticastPort = 19502;

        public static readonly byte[] MulticastMagic = Encoding.ASCII.GetBytes("LanChatMcastMsg_");
    };
}