using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatServer
{
    public class ChatServerClient
    {
        private ChatServer m_Server;
        private TcpClient m_Tcp;
        private Stream m_Stream;

        private BinaryWriter m_Writer;
        private BinaryReader m_Reader;
        public BinaryWriter Writer { get => m_Writer; }
        public BinaryReader Reader { get => m_Reader; }

        private List<ChatRoom> m_Rooms = new List<ChatRoom>();
        public List<ChatRoom> Rooms { get => m_Rooms; }

        private Thread m_Thread;
        public bool stopThread = false;

        private bool m_IsInServer = false;
        public bool IsInServer { get => m_IsInServer; set => m_IsInServer = value; }

        public bool disconnected = true;

        private string m_Nickname = "";
        public string Nickname { get => m_Nickname; set => m_Nickname = value; }

        public readonly object receiveSync = new object();
        public readonly object sendSync = new object();
        public readonly object threadStopSync = new object();
        public readonly object disconnectSync = new object();

        private readonly ChatServerPacketHandler m_PacketHandler;

        public ChatServerClient(TcpClient tcpClient, ChatServer server)
        {
            m_Tcp = tcpClient;
            m_Stream = tcpClient.GetStream();
            m_Server = server;
            m_PacketHandler = new ChatServerPacketHandler(this, m_Server);

            m_Writer = new BinaryWriter(m_Stream, Encoding.UTF8);
            m_Reader = new BinaryReader(m_Stream, Encoding.UTF8);

            m_Rooms = new List<ChatRoom>();

            IsInServer = false;

            lock(disconnectSync)
                disconnected = false;

            lock(threadStopSync)
                stopThread = false;

            m_Thread = new Thread(new ThreadStart(Run));
            m_Thread.Start();
        }

        public override string ToString() => Nickname;

        private void SendWelcome()
        {
            using (Packet packet = new Packet(PacketType.ServerWelcome))
            {
                lock (sendSync)
                {
                    packet.WriteToStream(Writer);
                    Writer.Flush();
                }
            }
        }

        private void SendServerClientList()
        {
            using (Packet packet = new Packet(PacketType.ServerClientList))
            {
                packet.Write(m_Server.ClientCount);

                m_Server.EnumerateClients((ChatServerClient client) =>
                {
                    packet.Write(client.Nickname);
                });

                lock (sendSync)
                {
                    packet.WriteToStream(Writer);
                    Writer.Flush();
                }
            }
        }

        private void SendServerRoomList()
        {
            using (Packet packet = new Packet(PacketType.ServerRoomList))
            {
                packet.Write(m_Server.RoomCount);

                m_Server.EnumerateRooms((ChatRoom room) =>
                {
                    packet.Write(room.name);

                    packet.Write(room.topic);

                    packet.Write(room.isEncrypted);
                });

                lock (sendSync)
                {
                    packet.WriteToStream(Writer);
                    Writer.Flush();
                }
            }
        }

        private bool PerformChecks()
        {
            Packet packet;

            using (packet = new Packet())
            {
                lock (receiveSync)
                    packet.ReadFromStream(Reader);

                if (packet.PacketType != PacketType.ClientHello)
                {
                    Console.WriteLine("Client did not send ClientHello packet");
                    Cleanup();
                    return false;
                }

                Nickname = packet.ReadString();
            }

            if (!m_Server.NicknameIsValid(Nickname))
            {
                using (packet = new Packet(PacketType.ServerError))
                {
                    packet.Write((uint)PacketErrorCode.EnteredUser);
                    packet.Write(Nickname + "zaten giriş yapmis.");

                    lock (sendSync)
                    {
                        packet.WriteToStream(Writer);
                        Writer.Flush();
                    }
                }
                Cleanup();
                return false;
            }

            return true;
        }

        private void HandleException(Exception e, string exceptionName)
        {
            Console.WriteLine($"{exceptionName}: {e.Message}");

            if (e.InnerException is not null)
                Console.WriteLine($"  InnerException: {e.InnerException.Message}");

            Console.WriteLine("StackTrace: " + e.StackTrace);

            lock (disconnectSync)
                disconnected = true;
        }

        public void Run()
        {
            try
            {
                if (!PerformChecks())
                    return;

                SendWelcome();

                SendServerClientList();

                SendServerRoomList();

                m_Server.AddClient(this);

                IsInServer = true;

                Packet packet = new Packet();

                while (true)
                {
                    lock (threadStopSync)
                    {
                        if (stopThread)
                            break;
                    }

                    lock (disconnectSync)
                    {
                        if (disconnected)
                            break;
                    }

                    lock (receiveSync)
                    {
                        if (!m_Tcp.Connected)
                            break;

                        packet.ReadFromStream(Reader);
                    }

                    m_PacketHandler.Handle(packet);
                }

                packet.Dispose();
            }
            catch (IOException e)
            {
                HandleException(e, "IOException");
            }
            catch (Exception e)
            {
                HandleException(e, "Exception");
            }

            lock (disconnectSync)
            {
                if (disconnected)
                {
                    Disconnect();
                }
            }
        }

        public void Cleanup()
        {
            lock (threadStopSync)
                stopThread = false;

            lock (disconnectSync)
                disconnected = true;

            IsInServer = false;

            lock (receiveSync)
                m_Reader.Close();
            lock (sendSync)
                m_Writer.Close();

            m_Stream.Close();
            m_Tcp.Close();
            m_Thread.Join();
        }

        public void Disconnect()
        {
            if (!string.IsNullOrEmpty(Nickname))
            { 
                Console.WriteLine(Nickname + " Ayrıldı.");
                m_Server.RemoveClient(this);
            }

            Cleanup();
        }
    }
}
