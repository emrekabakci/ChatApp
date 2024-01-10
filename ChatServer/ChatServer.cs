using System.Collections;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatServer
{
    public class ChatServer
    {
        public IPAddress hostAddress;
        public string hostAddressString;

        private Dictionary<string, ChatServerClient> m_Clients = new Dictionary<string, ChatServerClient>();

        private Dictionary<string, ChatRoom> m_Rooms = new Dictionary<string, ChatRoom>();

        private ChatServerMulticaster? m_Multicaster = null;

        private readonly object clientSync = new object();

        private readonly object roomSync = new object();

        public static readonly string MainRoomName = "Ana Oda";
        public static readonly string MainRoomTopic = "Ana oda.";

        public int ClientCount
        {
            get
            {
                lock (clientSync)
                    return m_Clients.Count;
            }
        }

        public void EnumerateClients(Action<ChatServerClient> action)
        {
            lock (clientSync)
            {
                foreach (ChatServerClient client in m_Clients.Values)
                {
                    action.Invoke(client);
                }
            }
        }

        public ChatServerClient? GetClient(string nickname)
        {
            lock (clientSync)
            {
                if (!m_Clients.ContainsKey(nickname))
                    return null;

                return m_Clients[nickname];
            }
        }

        public int RoomCount
        {
            get
            {
                lock (roomSync)
                    return m_Rooms.Count;
            }
        }

        public void EnumerateRooms(Action<ChatRoom> action)
        {
            lock (roomSync)
            {
                foreach (ChatRoom room in m_Rooms.Values)
                {
                    action.Invoke(room);
                }
            }
        }

        public void EnumerateRoomsUntil(Func<ChatRoom, bool> func)
        {
            lock (roomSync)
            {
                foreach (ChatRoom room in m_Rooms.Values)
                {
                    if (func.Invoke(room))
                        return;
                }
            }
        }

        public ChatRoom? GetRoom(string roomName)
        {
            lock (roomSync)
            {
                if (!m_Rooms.ContainsKey(roomName))
                    return null;

                return m_Rooms[roomName];
            }
        }

        public void Cleanup()
        {
            Console.WriteLine("Server kapatiliyor ...");

            if (m_Multicaster is not null)
                m_Multicaster.Stop();

            lock (clientSync)
            {
                foreach (ChatServerClient client in m_Clients.Values)
                {
                    client.Cleanup();
                }
            }
        }

        public void Run()
        {
            Console.WriteLine("Server aciliyor ...");

            IPAddress? hostIp = null;
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (hostIp is null)
                {
                    hostIp = ip;
                }
                else if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    hostIp = ip;
                    break;
                }
            }

            if (hostIp is null)
            {
                Console.WriteLine("Error: host adresi alinamadi");
                return;
            }

            hostAddress = hostIp;
            hostAddressString = hostAddress.ToString();


            m_Rooms.Clear();
            m_Rooms.Add(MainRoomName, new ChatRoom(null, MainRoomName, MainRoomTopic,
                                                   isEncrypted: false, isGlobal: true));

            TcpListener listener;
            try
            {
                listener = new TcpListener(IPAddress.Any, ChatConstants.ServerPort);
                listener.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Server baslatilamadi: {e.Message}");
                return;
            }

            Console.WriteLine($"Server bu IP adresi uzerinden dinleniyor {hostAddressString}:{ChatConstants.ServerPort}");

            m_Multicaster = new ChatServerMulticaster(this);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ProcessClient(client);
            }
        }

        private void ProcessClient(TcpClient tcpClient)
        {
            Console.WriteLine("Gelen baglanti kabul edildi.");
           
                ChatServerClient client = new ChatServerClient(tcpClient, this);
           
        }

        internal ChatRoom? CreateRoom(ChatServerClient owner, string name, string topic, bool encrypted)
        {
            lock (roomSync)
            {
                if (m_Rooms.ContainsKey(name))
                    return null;

                ChatRoom room = new ChatRoom(owner, name, topic, encrypted);
                m_Rooms.Add(name, room);

                using (Packet packet = new Packet(PacketType.ServerRoomCreated))
                {
                    packet.Write(room.name);
                    packet.Write(room.topic);
                    packet.Write(room.isEncrypted);

                    EnumerateClients((ChatServerClient client) =>
                    {
                        lock (client.sendSync)
                        {
                            packet.WriteToStream(client.Writer);
                            client.Writer.Flush();
                        }
                    });
                }

                AddClientToRoom(owner, room);

                return room;
            }
        }

        internal bool DeleteRoom(ChatRoom room)
        {
            lock (roomSync)
            {
                EnumerateClients((ChatServerClient client) =>
                {
                    client.Rooms.Remove(room);
                });

                if (!m_Rooms.Remove(room.name))
                    return false;

                using (Packet packet = new Packet(PacketType.ServerRoomDeleted))
                {
                    packet.Write(room.name);

                    EnumerateClients((ChatServerClient client) =>
                    {
                        lock (client.sendSync)
                        {
                            packet.WriteToStream(client.Writer);
                            client.Writer.Flush();
                        }
                    });
                }

                Console.WriteLine($"'{room.name}' odasi silindi");

                return true;
            }
        }

        internal void AddClientToRoom(ChatServerClient client, ChatRoom room)
        {
            lock (roomSync)
            {
                if (!room.IsGlobal && room.owner is null)
                {
                    if (room.clients.Count > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Non-global room '{room.name}' has clients in it but didn't have an owner.");
                        Console.ResetColor();
                    }

                    SetRoomOwner(room, client);
                }

                lock (clientSync)
                    room.AddClient(client);

                Console.WriteLine($"'{client} ' '{room.name} ' odasina katildi");
            }
        }

        internal void AddClient(ChatServerClient client)
        {
            lock (clientSync)
            {
                using (Packet packet = new Packet(PacketType.ServerClientJoin))
                {
                    packet.Write(client.ToString());

                    foreach (ChatServerClient client2 in m_Clients.Values)
                    {
                        lock (client2.sendSync)
                        {
                            packet.WriteToStream(client2.Writer);
                            client2.Writer.Flush();
                        }
                    }
                }

                AddClientToRoom(client, m_Rooms[MainRoomName]);

                m_Clients[client.ToString()] = client;
            }

            Console.WriteLine($"{client} Servera katildi.");
        }

        public void SetRoomOwner(ChatRoom room, ChatServerClient owner)
        {
            Console.WriteLine($"'{owner}' ' {room.name} ' odasinin sahibi");

            room.owner = owner;

            room.AddMessage(new ChatMessage(ChatMessageType.RoomOwnerChanged, room.owner.ToString()));

            using (Packet packet = new Packet(PacketType.ServerRoomOwnerChange))
            {
                packet.Write(room.name);
                packet.Write(room.owner.ToString());

                EnumerateClients((ChatServerClient client) =>
                {
                    lock (client.sendSync)
                    {
                        packet.WriteToStream(client.Writer);
                        client.Writer.Flush();
                    }
                });
            }
        }

        internal void RemoveClientFromRoom(ChatServerClient client, ChatRoom room)
        {
            lock (roomSync)
            {
                room.clients.Remove(client);

                lock (clientSync)
                    client.Rooms.Remove(room);

                room.AddMessage(new ChatMessage(ChatMessageType.UserLeaveRoom, client.ToString()));

                using (Packet packet = new Packet(PacketType.ServerClientRoomLeave))
                {
                    packet.Write(room.name);
                    packet.Write(client.ToString());

                    foreach (ChatServerClient client2 in room.clients)
                    {
                        lock (client2.sendSync)
                        {
                            packet.WriteToStream(client2.Writer);
                            client2.Writer.Flush();
                        }
                    }
                }

                if (client == room.owner)
                {
                    if (room.clients.Count > 0)
                    {
                        SetRoomOwner(room, room.clients.First());
                    }
                    else
                    {
                        if (room.isEncrypted)
                        {
                            Console.WriteLine($"Encrypted room '{room.name}' has no owner and is being deleted.");

                            DeleteRoom(room);
                        }
                        else
                        {
                            Console.WriteLine($"Room '{room.name}' has no owner.");

                            room.owner = null;
                        }

                        return;
                    }
                }
            }
        }

        internal bool RemoveClient(ChatServerClient client)
        {
            lock (clientSync)
            {
                bool rc = m_Clients.Remove(client.ToString());

                if (rc)
                    Console.WriteLine($"{client} serverdan ayrildi.");

                using (Packet packet = new Packet(PacketType.ServerClientLeave))
                {
                    packet.Write(client.ToString());

                    foreach (ChatServerClient client2 in m_Clients.Values)
                    {
                        lock (client2.sendSync)
                        {
                            packet.WriteToStream(client2.Writer);
                            client2.Writer.Flush();
                        }
                    }
                }

                List<ChatRoom> roomsTmp = new List<ChatRoom>(client.Rooms);
                foreach (ChatRoom room in roomsTmp)
                {
                    RemoveClientFromRoom(client, room);
                }

                return rc;
            }
        }

        internal bool NicknameIsValid(string nickname)
        {
            if (nickname.Length <= 0 || nickname.Length > 32)
                return false;

            lock (clientSync)
            {
                foreach (ChatServerClient client in m_Clients.Values)
                {
                    if (client.Nickname == nickname)
                        return false;
                }
            }

            return true;
        }
    }
}