using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatServer
{
    public class ChatRoom
    {
        public ChatServerClient? owner;

        public string name;

        public string topic;

        public HashSet<ChatServerClient> clients = new HashSet<ChatServerClient>();

        public bool isEncrypted;

        private bool m_IsGlobal;

        public bool IsGlobal { get => m_IsGlobal; }

        private List<ChatMessage> m_Messages;

        public List<ChatMessage> Messages { get => m_Messages; }

        public ChatRoom(ChatServerClient? owner, string name, string topic, bool isEncrypted = false, bool isGlobal = false)
        {
            this.owner = owner;
            this.name = name;
            this.topic = topic;
            this.isEncrypted = isEncrypted;

            this.m_IsGlobal = isGlobal;
            this.m_Messages = new List<ChatMessage>();

            clients.Clear();

            if (owner is not null)
            {
                m_Messages.Add(new ChatMessage(ChatMessageType.RoomCreated, owner.Nickname));

                if (!string.IsNullOrEmpty(topic))
                    m_Messages.Add(new ChatMessage(ChatMessageType.RoomTopicSet, owner.Nickname, topic));
            }
        }

        public void AddClient(ChatServerClient client)
        {
            if (clients.Contains(client))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Client '{client}' already exists in room '{name}'");
                Console.ResetColor();
                return;
            }

            ChatMessage msg = new ChatMessage(ChatMessageType.UserJoinRoom, client.ToString());
            AddMessage(msg);

            clients.Add(client);

            client.Rooms.Add(this);

            using (Packet packet = new Packet(PacketType.ServerClientRoomJoin))
            {
                packet.Write(this.name);
                packet.Write(client.ToString());

                foreach (ChatServerClient client2 in this.clients)
                {
                    lock (client2.sendSync)
                    {
                        packet.WriteToStream(client2.Writer);
                        client2.Writer.Flush();
                    }
                }
            }

            using (Packet packet = new Packet(PacketType.ServerClientRoomMembers))
            {
                packet.Write(this.name);
                packet.Write(this.clients.Count);

                foreach (ChatServerClient client2 in this.clients)
                {
                    packet.Write(client2.ToString());
                }

                lock (client.sendSync)
                {
                    packet.WriteToStream(client.Writer);
                    client.Writer.Flush();
                }
            }

            using (Packet packet = new Packet(PacketType.ServerClientRoomMessages))
            {
                packet.Write(this.name);
                packet.Write(this.Messages.Count);

                foreach (ChatMessage message in this.Messages)
                {
                    packet.Write((int)message.type);
                    packet.Write(message.author);

                    if (message.message is not null)
                    {
                        packet.Write(message.message);

                        if (this.isEncrypted && message.type == ChatMessageType.UserMessage)
                        {
                            if (message.ivString is not null)
                                packet.Write(message.ivString);
                            else
                                packet.Write(string.Empty);
                        }
                    }
                    else
                    {
                        packet.Write(string.Empty);
                    }
                }

                lock (client.sendSync)
                {
                    packet.WriteToStream(client.Writer);
                    client.Writer.Flush();
                }
            }
        }

        public void AddMessage(ChatMessage message)
        {
            Messages.Add(message);

            using (Packet packet = new Packet(PacketType.ServerRoomMessageReceived))
            {
                packet.Write(this.name);

                packet.Write((int)message.type);
                packet.Write(message.author);

                if (message.message is not null)
                {
                    packet.Write(message.message);

                    if (this.isEncrypted && message.type == ChatMessageType.UserMessage)
                    {
                        if (message.ivString is not null)
                            packet.Write(message.ivString);
                        else
                            packet.Write(string.Empty);
                    }
                }
                else
                {
                    packet.Write(string.Empty);
                }

                foreach (ChatServerClient client in this.clients)
                {
                    lock (client.sendSync)
                    {
                        packet.WriteToStream(client.Writer);
                        client.Writer.Flush();
                    }
                }
            }
        }
    }
}