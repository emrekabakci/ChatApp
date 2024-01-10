using System.Security.Cryptography;
using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatClient
{
    public class ChatClientPacketHandler
    {
        private readonly ChatClient m_Client;

        private delegate void PacketHandler(Packet packet);

        private readonly Dictionary<PacketType, PacketHandler> m_Handlers;

        public ChatClientPacketHandler(ChatClient client)
        {
            this.m_Client = client;

            m_Handlers = new()
            {
                { PacketType.ServerWelcome, ServerWelcome },
                { PacketType.ServerError, ServerError },
                { PacketType.ServerClientList, ServerClientList },
                { PacketType.ServerRoomList, ServerRoomList },
                { PacketType.ServerClientRoomMembers, ServerClientRoomMembers },
                { PacketType.ServerClientRoomMessages, ServerClientRoomMessages },
                { PacketType.ServerClientJoin, ServerClientJoin },
                { PacketType.ServerClientLeave, ServerClientLeave },
                { PacketType.ServerClientRoomJoin, ServerClientRoomJoin },
                { PacketType.ServerClientRoomLeave, ServerClientRoomLeave },
                { PacketType.ServerDirectMessageReceived, ServerDirectMessageReceived },
                { PacketType.ServerRoomMessageReceived, ServerRoomMessageReceived },
                { PacketType.ServerRoomCreated, ServerRoomCreated },
                { PacketType.ServerRoomDeleted, ServerRoomDeleted },
                { PacketType.ServerRoomCreateError, ServerRoomCreateError },
                { PacketType.ServerRoomDeleteError, ServerRoomDeleteError },
                { PacketType.ServerRoomOwnerChange, ServerRoomOwnerChange },
                { PacketType.ServerClientJoinEncryptedRoomRequest, ServerClientJoinEncryptedRoomRequest },
                { PacketType.ServerClientEncryptedRoomAuthorise, ServerClientEncryptedRoomAuthorise },
                { PacketType.ServerClientEncryptedRoomAuthoriseFail, ServerClientEncryptedRoomAuthoriseFail },
            };
        }

        private object Invoke(Delegate? method, params object[] args)
        {
            if (method is null)
                return false;

            return m_Client.Form.Invoke(method, args);
        }

        public void Handle(Packet packet)
        {
            if (!m_Handlers.ContainsKey(packet.PacketType))
            {
                ShowError("Unhandled packet type " + packet.PacketType.ToString());
                return;
            }

            packet.Lock();

            m_Handlers[packet.PacketType](packet);

            packet.Unlock();
        }

        private void ShowError(string message)
        {
            Invoke(m_Client.OnError, message);
        }

        private void ServerWelcome(Packet packet)
        {
            m_Client.InServer = true;

            Invoke(m_Client.OnConnectionSuccess);
        }

        private void ServerError(Packet packet)
        {
            PacketErrorCode code = (PacketErrorCode)packet.ReadUInt32();
            string msg = packet.ReadString();

            ShowError(msg);
        }

        private void ServerClientList(Packet packet)
        {
            int count = packet.ReadInt32();

            foreach (ChatRecipient client in m_Client.Clients.Values)
            {
                client.isJoined = false;
            }

            for (int i = 0; i < count; ++i)
            {
                string nickname = packet.ReadString();

                if (nickname == m_Client.ToString())
                    continue;

                if (!m_Client.Clients.ContainsKey(nickname))
                {
                    ChatRecipient addedRecipient = new ChatRecipient(nickname, true);
                    m_Client.Clients.Add(nickname, addedRecipient);

                    ChatDirectChannel channel = new ChatDirectChannel(addedRecipient);
                    m_Client.Channels.Add(channel);
                }
                else
                {
                    m_Client.Clients[nickname].isJoined = true;
                }
            }

            Invoke(m_Client.OnChannelListUpdate);
        }

        private void ServerRoomList(Packet packet)
        {
            int count = packet.ReadInt32();

            foreach (ChatChannel channel in m_Client.Channels)
            {
                if (!channel.IsDirect)
                    m_Client.Channels.Remove(channel);
            }

            for (int i = 0; i < count; ++i)
            {
                string roomName = packet.ReadString();
                string roomTopic = packet.ReadString();
                bool roomEncrypted = packet.ReadBool();

                ChatRoomChannel channel = new ChatRoomChannel(roomName, roomTopic, roomEncrypted);
                m_Client.Channels.Add(channel);
            }

            Invoke(m_Client.OnChannelListUpdate);
        }

        private ChatRoomChannel? FindRoom(string roomName)
        {
            foreach (ChatChannel channel in m_Client.Channels)
            {
                if (channel.IsRoomChannel(roomName))
                    return (ChatRoomChannel)channel;
            }

            return null;
        }

        private ChatRoomChannel? FindOwnedRoom(string roomName)
        {
            foreach (ChatChannel channel in m_Client.OwnedRooms)
            {
                if (channel.IsRoomChannel(roomName))
                    return (ChatRoomChannel)channel;
            }

            return null;
        }

        private ChatDirectChannel? FindDirectMessageChannel(string userName)
        {
            foreach (ChatChannel channel in m_Client.Channels)
            {
                if (channel.IsDirectChannel(userName))
                    return (ChatDirectChannel)channel;
            }

            return null;
        }

        private void ServerClientRoomMembers(Packet packet)
        {
            string roomName = packet.ReadString();
            int memberCount = packet.ReadInt32();

            ChatChannel? channel = FindRoom(roomName);
            if (channel is null)
            {
                ShowError($"Got members for unknown room '{roomName}'");
                return;
            }

            channel.recipients.Clear();

            for (int i = 0; i < memberCount; ++i)
            {
                string clientName = packet.ReadString();

                if (clientName == m_Client.ToString())
                {
                    ((ChatRoomChannel)channel).isJoined = true;
                    continue;
                }

                if (!m_Client.Clients.ContainsKey(clientName))
                {
                    ShowError($"Room contains unknown user {clientName}");
                    continue;
                }

                channel.recipients.Add(m_Client.Clients[clientName]);
            }
        }

        private string TryDecryptRoomMessage(ChatRoomChannel room, string iv, string message)
        {
            string finalMessage;

            if (string.IsNullOrEmpty(iv))
            {
                finalMessage = "!! failed to decrypt (missing IV) !!";
            }
            else
            {
                if (!m_Client.RoomKeychain.ContainsKey(room.roomName))
                {
                    ShowError($"No key for room '{room.roomName}'");
                    finalMessage = "!! failed to decrypt (missing key) !!";
                }
                else
                {
                    byte[]? roomKey = m_Client.RoomKeychain[room.roomName];
                    if (roomKey is null)
                    {
                        ShowError($"No key for room '{room.roomName}'");
                        finalMessage = "!! failed to decrypt (missing key) !!";
                    }
                    else
                    {
                        CryptoCipher cipher = new CryptoCipher(iv, message);
                        finalMessage = ChatClientCrypto.DecryptMessage(cipher, roomKey, out _);
                    }
                }
            }

            return finalMessage;
        }

        private ChatMessage ReadRoomMessage(Packet packet, ChatRoomChannel room)
        {
            int messageType = packet.ReadInt32();
            string messageAuthor = packet.ReadString();
            string messageString = packet.ReadString();

            string? finalMessage;

            if (string.IsNullOrEmpty(messageString))
            {
                finalMessage = null;
            }
            else
            {
                if (!room.isEncrypted ||
                    (ChatMessageType)messageType != ChatMessageType.UserMessage)
                {
                    finalMessage = messageString;
                }
                else
                {
                    string ivString = packet.ReadString();
                    finalMessage = TryDecryptRoomMessage(room, messageString, ivString);
                }
            }

            return new ChatMessage((ChatMessageType)messageType, messageAuthor, finalMessage);
        }

        private void ServerClientRoomMessages(Packet packet)
        {
            string roomName = packet.ReadString();
            int messageCount = packet.ReadInt32();

            ChatRoomChannel? channel = FindRoom(roomName);
            if (channel is null)
            {
                ShowError($"Got messages for unknown room '{roomName}'");
                return;
            }

            channel.messages.Clear();

            for (int i = 0; i < messageCount; ++i)
            {
                ChatMessage message = ReadRoomMessage(packet, channel);

                channel.AddMessage(message);
            }

            Invoke(m_Client.OnRoomMessageListReceived, channel);
        }

        private void ServerClientJoin(Packet packet)
        {
            string nickname = packet.ReadString();

            if (nickname == m_Client.ToString())
                return;

            if (!m_Client.Clients.ContainsKey(nickname))
            {
                ChatRecipient joinedRecipient = new ChatRecipient(nickname, true);
                m_Client.Clients.Add(nickname, joinedRecipient);

                m_Client.Channels.Add(new ChatDirectChannel(joinedRecipient));
            }
            else
            {
                m_Client.Clients[nickname].isJoined = true;
            }

            ChatDirectChannel? channel = FindDirectMessageChannel(nickname);
            if (channel is not null)
            {
                ChatMessage msg = channel.AddMessage(ChatMessageType.UserJoin, nickname);
                Invoke(m_Client.OnMessageReceived, channel, msg);
            }

            Invoke(m_Client.OnChannelListUpdate);

            Invoke(m_Client.OnClientJoin, m_Client.Clients[nickname]);

            return;
        }

        private void ServerClientLeave(Packet packet)
        {
            string nickname = packet.ReadString();

            if (nickname == m_Client.ToString())
                return;

            if (!m_Client.Clients.ContainsKey(nickname))
                return;

            m_Client.Clients[nickname].isJoined = false;

            ChatDirectChannel? channel = FindDirectMessageChannel(nickname);
            if (channel is not null)
            {
                ChatMessage msg = channel.AddMessage(ChatMessageType.UserLeave, nickname);
                Invoke(m_Client.OnMessageReceived, channel, msg);
            }

            Invoke(m_Client.OnClientLeave, m_Client.Clients[nickname]);
        }

        private void ServerClientRoomJoin(Packet packet)
        {
            string roomName = packet.ReadString();
            string nickname = packet.ReadString();

            ChatRoomChannel? room = FindRoom(roomName);
            if (room is null)
            {
                ShowError($"Client '{nickname}' joined unknown room '{roomName}'.");
                return;
            }

            if (room.ContainsRecipient(nickname))
                return;

            if (nickname == m_Client.ToString())
            {
                room.isJoined = true;

                m_Client.OwnedRooms.Add(room);

                Invoke(m_Client.OnRoomCreateSuccess);

                return;
            }

            if (!m_Client.Clients.ContainsKey(nickname))
            {
                ShowError($"Unknown client '{nickname}' joined room '{roomName}'.");
                return;
            }

            room.recipients.Add(m_Client.Clients[nickname]);
        }

        private void ServerClientRoomLeave(Packet packet)
        {
            string roomName = packet.ReadString();
            string nickname = packet.ReadString();

            if (nickname == m_Client.ToString())
                return;

            ChatRoomChannel? room = FindRoom(roomName);
            if (room is null)
                return;

            if (!m_Client.Clients.ContainsKey(nickname) ||
                !room.ContainsRecipient(nickname))
            {
                return;
            }

            room.recipients.Remove(m_Client.Clients[nickname]);
        }

        private void ServerDirectMessageReceived(Packet packet)
        {
            string sender = packet.ReadString();

            string recipient = packet.ReadString();

            string messageString = packet.ReadString();

            string channelName;
            if (sender == m_Client.ToString())
                channelName = recipient;
            else
                channelName = sender;

            ChatChannel? channel = FindDirectMessageChannel(channelName);
            if (channel is null)
            {
                ShowError($"Received message on unknown DM {channelName}.");
                return;
            }

            ChatMessage message = new ChatMessage(ChatMessageType.UserMessage, sender, messageString);
            channel.AddMessage(message);

            if (!(bool)Invoke(m_Client.OnMessageReceived, channel, message))
            {
                if (message.type == ChatMessageType.UserMessage)
                    ++channel.unreadMessages;

                Invoke(m_Client.OnChannelListUpdate);
            }
        }

        private void ServerRoomMessageReceived(Packet packet)
        {
            string roomName = packet.ReadString();
            ChatRoomChannel? channel = FindRoom(roomName);
            if (channel is null)
            {
                ShowError($"Received message on unknown room {roomName}.");
                return;
            }

            ChatMessage message = ReadRoomMessage(packet, channel);

            channel.AddMessage(message);

            if (!(bool)Invoke(m_Client.OnMessageReceived, channel, message))
            {
                if (message.type == ChatMessageType.UserMessage)
                    ++channel.unreadMessages;

                Invoke(m_Client.OnChannelListUpdate);
            }
        }

        private void ServerRoomCreated(Packet packet)
        {
            string roomName = packet.ReadString();
            string roomTopic = packet.ReadString();
            bool roomEncrypted = packet.ReadBool();

            ChatRoomChannel channel = new ChatRoomChannel(roomName, roomTopic, roomEncrypted);
            m_Client.Channels.Add(channel);

            Invoke(m_Client.OnChannelListUpdate);
        }

        private void ServerRoomDeleted(Packet packet)
        {
            string roomName = packet.ReadString();

            ChatRoomChannel? channel = FindRoom(roomName);

            if (channel is null)
                return;

            if (m_Client.Channel == channel)
                m_Client.Channel = null;

            m_Client.OwnedRooms.Remove(channel);

            m_Client.Channels.Remove(channel);

            Invoke(m_Client.OnChannelListUpdate);
        }

        private void ServerRoomCreateError(Packet packet)
        {
            PacketErrorCode code = (PacketErrorCode)packet.ReadUInt32();
            string msg = packet.ReadString();

            Invoke(m_Client.OnRoomCreateFail, msg);
        }

        private void ServerRoomDeleteError(Packet packet)
        {
            PacketErrorCode code = (PacketErrorCode)packet.ReadUInt32();
            string msg = packet.ReadString();

            Invoke(m_Client.OnRoomDeleteFail, msg);
        }

        private void ServerRoomOwnerChange(Packet packet)
        {
            string roomName = packet.ReadString();
            string ownerName = packet.ReadString();

            ChatRoomChannel? room = FindRoom(roomName);
            if (room is null)
            {
                return;
            }

            if (ownerName == m_Client.ToString())
            {
                m_Client.OwnedRooms.Add(room);
            }
        }

        private void ServerClientJoinEncryptedRoomRequest(Packet packet)
        {
            string roomName = packet.ReadString();
            string nickname = packet.ReadString();
            string saltString = packet.ReadString();
            string ivString = packet.ReadString();
            string cipherMessageString = packet.ReadString();

            ChatRoomChannel? room = FindOwnedRoom(roomName);
            if (room is null)
            {
                using (Packet packet2 = new Packet(PacketType.ClientEncryptedRoomAuthoriseFail))
                {
                    packet2.Write(roomName);
                    packet2.Write(nickname);
                    packet2.Write((int)PacketErrorCode.UnknownError);
                    packet2.WriteToStream(m_Client.Writer);
                    m_Client.Writer.Flush();
                }
                return;
            }

            byte[]? roomKey = null;
            if (m_Client.RoomKeychain.ContainsKey(room.roomName))
                roomKey = m_Client.RoomKeychain[room.roomName];

            if (roomKey is null)
                return;   

            string expectedMessage = nickname + roomName + saltString;

            CryptoCipher cipher = new CryptoCipher(ivString, cipherMessageString);
            string plainText = ChatClientCrypto.DecryptMessage(cipher, roomKey, out bool failed);

            if (failed || plainText != expectedMessage)
            {
                using (Packet packet2 = new Packet(PacketType.ClientEncryptedRoomAuthoriseFail))
                {
                    packet2.Write(roomName);
                    packet2.Write(nickname);
                    packet2.Write((int)PacketErrorCode.PasswordMismatch);
                    packet2.WriteToStream(m_Client.Writer);
                    m_Client.Writer.Flush();
                }

                return;
            }

            using (Packet packet2 = new Packet(PacketType.ClientEncryptedRoomAuthorise))
            {
                packet2.Write(roomName);
                packet2.Write(nickname);
                packet2.WriteToStream(m_Client.Writer);
                m_Client.Writer.Flush();
            }
        }

        private void ServerClientEncryptedRoomAuthorise(Packet packet)
        {
            lock (m_Client.passwordAwaitSync)
                m_Client.passwordAwait = ChatClientPasswordAwaitState.Successful;
        }

        private void ServerClientEncryptedRoomAuthoriseFail(Packet packet)
        {
            packet.ReadString();

            PacketErrorCode code = (PacketErrorCode)packet.ReadInt32();

            lock (m_Client.passwordAwaitSync)
            {
                if (code == PacketErrorCode.PasswordMismatch)
                    m_Client.passwordAwait = ChatClientPasswordAwaitState.Incorrect;
                else
                    m_Client.passwordAwait = ChatClientPasswordAwaitState.UnknownError;
            }
        }
    }
}