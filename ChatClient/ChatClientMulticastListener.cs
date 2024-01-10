using ChatUygulamasi.ChatShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ChatUygulamasi.ChatClient
{
    public class ChatClientMulticastListener
    {
        private HashSet<string> m_Servers = new HashSet<string>();

        public Action<HashSet<string>>? ServerListChanged = null;

        private Thread? m_Thread = null;
        private bool m_Running = false;

        public bool Running
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

        private Socket? m_Socket = null;
        private IPEndPoint? m_Endpoint = null;

        private object m_Sync = new object();

        public void Start()
        {
            lock (m_Sync)
                m_Servers.Clear();

            if (ServerListChanged is not null)
                ServerListChanged(m_Servers);

            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            m_Endpoint = new IPEndPoint(IPAddress.Any, ChatConstants.MulticastPort);
            m_Socket.Bind(m_Endpoint);

            m_Socket.SetSocketOption(SocketOptionLevel.IP,
                                     SocketOptionName.AddMembership,
                                     new MulticastOption(ChatConstants.MulticastIP, IPAddress.Any));

            Running = true;
            m_Thread = new Thread(new ThreadStart(Run));
            m_Thread.IsBackground = true;
            m_Thread.Start();
        }

        public void Stop()
        {
            lock (m_Sync)
                m_Servers.Clear();

            if (m_Socket is not null)
                m_Socket.Close();

            Running = false;
        }

        private void Run()
        {
            byte[] buffer = new byte[1024];

            while (Running)
            {
                if (m_Socket is null)
                    break;

                try
                {
                    m_Socket.Receive(buffer);
                }
                catch (SocketException)
                {
                    continue;
                }
                catch (Exception)
                {
                    break;
                }

                try
                {
                    string hostname = "";
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            byte[] magic = br.ReadBytes(ChatConstants.MulticastMagic.Length);

                            if (!magic.SequenceEqual(ChatConstants.MulticastMagic))
                            {
                                int len1 = magic.Length;
                                int len3 = ChatConstants.MulticastMagic.Length;
                                continue;
                            }

                            hostname = br.ReadString();
                        }
                    }

                    if (!string.IsNullOrEmpty(hostname))
                    {
                        lock (m_Sync)
                            m_Servers.Add(hostname);

                        if (ServerListChanged is not null)
                            ServerListChanged(m_Servers);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            lock (m_Sync)
                m_Servers.Clear();

            if (ServerListChanged is not null)
                ServerListChanged(m_Servers);
        }
    }
}