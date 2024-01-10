using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatServer
{
    public class ChatServerPacketHandler
    {
        private readonly ChatServerClient m_Client;
        private readonly ChatServer m_Server;

        private delegate void PacketHandler(Packet packet);

        private readonly Dictionary<PacketType, PacketHandler> m_Handlers;

        public ChatServerPacketHandler(ChatServerClient client, ChatServer server)
        {
            this.m_Client = client;
            this.m_Server = server;

            m_Handlers = new Dictionary<PacketType, PacketHandler>()
            {
                { PacketType.ClientDisconnect, ClientDisconnect },
                { PacketType.ClientDirectMessage, ClientDirectMessage },
                { PacketType.ClientRoomMessage, ClientRoomMessage },
                { PacketType.ClientRoomCreate, ClientRoomCreate },
                { PacketType.ClientRoomDelete, ClientRoomDelete },
                { PacketType.ClientRoomJoin, ClientRoomJoin },
                { PacketType.ClientEncryptedRoomAuthorise, ClientEncryptedRoomAuthorise },
                { PacketType.ClientEncryptedRoomAuthoriseFail, ClientEncryptedRoomAuthoriseFail },
            };
        }

        public void Handle(Packet packet)
        {
            if (!m_Handlers.ContainsKey(packet.PacketType))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unhandled packet " + packet.PacketType.ToString());
                Console.ResetColor();
                return;
            }

            packet.Lock();

            m_Handlers[packet.PacketType](packet);

            packet.Unlock();
        }

        private void ClientDisconnect(Packet packet)
        {
            lock (m_Client.disconnectSync)
                m_Client.disconnected = true;
        }

        private void ClientDirectMessage(Packet packet)
        {
            string recipientName = packet.ReadString();
            string msg = packet.ReadString();

            Console.WriteLine($"'{m_Client}' --> '{recipientName}': '{msg}'");

            using (Packet packet2 = new Packet(PacketType.ServerDirectMessageReceived))
            {
                packet2.Write(m_Client.ToString());
                packet2.Write(recipientName);
                packet2.Write(msg);

                ChatServerClient? recipient = m_Server.GetClient(recipientName);
                if (recipient is not null)
                {
                    lock (recipient.sendSync)
                    {
                        packet2.WriteToStream(recipient.Writer);
                        recipient.Writer.Flush();
                    }
                }

                lock (m_Client.sendSync)
                {
                    packet2.WriteToStream(m_Client.Writer);
                    m_Client.Writer.Flush();
                }
            }
        }

        private void ClientRoomMessage(Packet packet)
        {
            string roomName = packet.ReadString();
            string msg = packet.ReadString();

            Console.WriteLine($"'{m_Client}' --> '{roomName}' (room): '{msg}'");

            ChatRoom? room = m_Server.GetRoom(roomName);
            if (room is null)
                return;

            if (!m_Client.Rooms.Contains(room))
            {
                Console.WriteLine($"Error: User '{m_Client}' attempt to " +
                                  $"write to room '{roomName}' which they are not a member of.");
                return;
            }

            string? iv = null;
            if (room.isEncrypted)
                iv = packet.ReadString();

            room.AddMessage(new ChatMessage(ChatMessageType.UserMessage, m_Client.Nickname, msg, iv));
        }

        private void ClientRoomCreate(Packet packet)
        {
            string roomName = packet.ReadString();
            string roomTopic = packet.ReadString();
            bool roomEncrypted = packet.ReadBool();

            if (string.IsNullOrEmpty(roomName))
                return;

            ChatRoom? room = m_Server.CreateRoom(m_Client, roomName, roomTopic, roomEncrypted);

            if (room is null)
            {
                using (Packet error = new Packet(PacketType.ServerRoomCreateError))
                {
                    error.Write((uint)PacketErrorCode.RoomNameTaken);
                    error.Write("Room name is already taken.  Please enter a different name.");

                    lock (m_Client.sendSync)
                    {
                        error.WriteToStream(m_Client.Writer);
                        m_Client.Writer.Flush();
                    }
                }

                return;
            }

            if (roomEncrypted)
                Console.WriteLine($"'{m_Client}' created encrypted room '{roomName}' with topic '{roomTopic}'");
            else
                Console.WriteLine($"'{m_Client}' created room '{roomName}' with topic '{roomTopic}'");
        }

        private ChatRoom? GetRoomByName(string roomName)
        {
            ChatRoom? result = null;

            m_Server.EnumerateRoomsUntil((ChatRoom room) =>
            {
                if (room.name == roomName)
                {
                    result = room;
                    return true;
                }
                return false;
            });

            return result;
        }

        private void ClientRoomDelete(Packet packet)
        {
            string roomName = packet.ReadString();

            if (string.IsNullOrEmpty(roomName))
                return;

            ChatRoom? room = GetRoomByName(roomName);
            if (room is null)
            {
                Console.WriteLine($"'{m_Client}' attempted to delete unknown room '{roomName}'");
                return;
            }

            if (room.owner != m_Client)
            {
                Console.WriteLine($"'{m_Client}' attempted to delete room " +
                                  $"'{roomName}' that they do not own.");
                return;
            }

            m_Server.DeleteRoom(room);
        }

        private void ClientRoomJoin(Packet packet)
        {
            string roomName = packet.ReadString();

            Console.WriteLine($"'{m_Client}' joining room {roomName} ...");

            ChatRoom? room = GetRoomByName(roomName);
            if (room is null)
            {
                Console.WriteLine($"'{m_Client}' attempted to join unknown room {roomName}");
                return;
            }

            if (!room.isEncrypted)
            {
                m_Server.AddClientToRoom(m_Client, room);
            }
            else
            {
                if (room.owner is null)
                {
                    Console.WriteLine($"'{m_Client}' attempted to join encrypted room with no owner.");
                    return;
                }

                string saltString = packet.ReadString();
                string ivString = packet.ReadString();
                string messageString = packet.ReadString();

                Console.WriteLine($"'{m_Client}' attempting to join encrypted room '{roomName}'");
                Console.WriteLine($"'{m_Client}' salt:{saltString} iv:{ivString} message:{messageString}");

                using (Packet packet2 = new Packet(PacketType.ServerClientJoinEncryptedRoomRequest))
                {
                    packet2.Write(roomName);
                    packet2.Write(m_Client.ToString());
                    packet2.Write(saltString);
                    packet2.Write(ivString);
                    packet2.Write(messageString);

                    lock (room.owner.sendSync)
                    {
                        packet2.WriteToStream(room.owner.Writer);
                        room.owner.Writer.Flush();
                    }
                }
            }
        }

        private void DontAuthoriseClient(ChatServerClient client, ChatRoom room, PacketErrorCode errorCode)
        {
            using (Packet packet = new Packet(PacketType.ServerClientEncryptedRoomAuthoriseFail))
            {
                packet.Write(room.name);
                packet.Write((int)errorCode);

                lock (client.sendSync)
                {
                    packet.WriteToStream(client.Writer);
                    client.Writer.Flush();
                }
            }
        }

        private bool EncryptedRoomAuthoriseChecks(string roomName, string nickname,
                                                  out ChatServerClient? client, out ChatRoom? room,
                                                  bool noAuth)
        {
            string authStr = noAuth ? "NOT authorise" : "authorise";

            Console.WriteLine($"'{m_Client}' attempting to {authStr} '{nickname}' to '{roomName}' ...");

            client = null;
            room = null;

            client = m_Server.GetClient(nickname);
            if (client is null)
            {
                Console.WriteLine($"'{m_Client}' attempted to {authStr} " +
                                  $"unknown user '{nickname}' to room '{roomName}'");
                return false;
            }

            room = GetRoomByName(roomName);
            if (room is null)
            {
                Console.WriteLine($"'{m_Client}' attempted to {authStr} " +
                                  $"'{nickname}' to unknown room '{roomName}'");
                return false;
            }

            if (room.owner != m_Client)
            {
                Console.WriteLine($"'{m_Client}' attempted to {authStr} " +
                                  $"'{nickname}' to room '{roomName}' which they do not own.");
                DontAuthoriseClient(client, room, PacketErrorCode.UnknownError);
                return false;
            }

            if (!room.isEncrypted)
            {
                Console.WriteLine($"'{m_Client}' attempted to {authStr} " +
                                  $"'{nickname}' to non-encrypted room '{roomName}'");
                DontAuthoriseClient(client, room, PacketErrorCode.UnknownError);
                return false;
            }

            Console.WriteLine($"'{m_Client}' did {authStr} '{nickname}' to '{roomName}' ...");

            return true;
        }

        private void ClientEncryptedRoomAuthorise(Packet packet)
        {
            string roomName = packet.ReadString();
            string nickname = packet.ReadString();

            ChatServerClient? client;
            ChatRoom? room;
            if (!EncryptedRoomAuthoriseChecks(roomName, nickname, out client, out room, false) ||
                room is null || client is null)
                return;

            using (Packet packet2 = new Packet(PacketType.ServerClientEncryptedRoomAuthorise))
            {
                packet2.Write(room.name);

                lock (client.sendSync)
                {
                    packet2.WriteToStream(client.Writer);
                    client.Writer.Flush();
                }
            }

            m_Server.AddClientToRoom(client, room);

            return;
        }

        private void ClientEncryptedRoomAuthoriseFail(Packet packet)
        {
            string roomName = packet.ReadString();
            string nickname = packet.ReadString();
            int errorCode = packet.ReadInt32();

            ChatServerClient? client;
            ChatRoom? room;
            if (!EncryptedRoomAuthoriseChecks(roomName, nickname, out client, out room, false) ||
                room is null || client is null)
            {
                return;
            }

            DontAuthoriseClient(client, room, (PacketErrorCode)errorCode);
        }
    }
}