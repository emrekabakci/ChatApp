using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatServer
{
    internal class ChatServerMulticaster
    {
        private Thread m_Thread;

        private Socket m_Socket;
        private IPEndPoint m_Endpoint;

        private readonly object m_Sync = new object();

        private bool m_Running = false;

        private bool Running
        {
            get
            {
                lock (m_Sync)
                    return m_Running;
            }
            set
            {
                lock (m_Sync)
                    m_Running = value;
            }
        }

        private ChatServer m_Server;

        private static readonly int MulticastDelay = 4000;

        public ChatServerMulticaster(ChatServer server)
        {
            m_Server = server;

            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            m_Socket.SetSocketOption(SocketOptionLevel.IP,
                                     SocketOptionName.AddMembership,
                                     new MulticastOption(ChatConstants.MulticastIP));

            m_Socket.SetSocketOption(SocketOptionLevel.IP,
                                     SocketOptionName.MulticastTimeToLive,
                                     2);

            m_Endpoint = new IPEndPoint(ChatConstants.MulticastIP, ChatConstants.MulticastPort);

            m_Socket.Connect(m_Endpoint);

            Running = true;

            m_Thread = new Thread(new ThreadStart(Run));
            m_Thread.IsBackground = true;
            m_Thread.Start();

            Console.WriteLine("Server multicaster tanimlandi.");
        }

        public void Stop()
        {
            m_Socket.Close();

            Running = false;
        }

        private byte[] GetMessage()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(ChatConstants.MulticastMagic);
                    bw.Write(m_Server.hostAddressString);
                }
                return ms.GetBuffer();
            }
        }

        private void Run()
        {
            byte[] message = GetMessage();

            while (Running)
            {
                try
                {
                    m_Socket.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception in multicaster: {e.Message}");
                    Console.WriteLine($"Stacktrace: {e.StackTrace}");
                    Console.WriteLine("Multicaster thread will now exit.");
                    break;
                }

                Thread.Sleep(MulticastDelay);
            }
        }
    }
}