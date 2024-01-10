using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatClient
{
    public class ChatDirectChannel : ChatChannel
    {
        public ChatRecipient Recipient { get => recipients[0]; }

        public override string DisplayString
        {
            get
            {
                if (Recipient.isJoined)
                {
                    if (unreadMessages > 0)
                        return $"({unreadMessages}) {Recipient.nickname}";

                    return Recipient.nickname + " (Online)";
                }
                else
                {
                    DateTime dateTime = DateTime.Now;
                    if (unreadMessages > 0)
                        return $"({unreadMessages}) {Recipient.nickname} (Offline) ";

                    return Recipient.nickname + " (Offline) " + "Son görülme: " + dateTime;
                }
            }
        }

        public override bool IsDirect { get => true; }

        public override bool IsRoomChannel(string roomName) => false;

        public override bool IsDirectChannel(string recipient) => this.Recipient.nickname == recipient;

        public ChatDirectChannel(ChatRecipient recipient)
        {
            this.recipients = new List<ChatRecipient>() { recipient };
            this.messages = new List<ChatMessage>();
            this.unreadMessages = 0;
        }
    }

    public class ChatRoomChannel : ChatChannel
    {
        public string roomName;

        public string roomTopic;

        public bool isEncrypted;

        public byte[]? roomKey;

        public bool isJoined;

        public override string DisplayString
        {
            get
            {
                if (unreadMessages > 0)
                    return $"({unreadMessages}) {roomName}";

                return $"{roomName}";
            }
        }

        public override bool IsDirect { get => false; }

        public override bool IsRoomChannel(string roomName) => this.roomName == roomName;

        public override bool IsDirectChannel(string roomName) => false;

        public ChatRoomChannel(string roomName, string roomTopic, bool roomEncrypted)
        {
            this.recipients = new List<ChatRecipient>();
            this.roomName = roomName;
            this.roomTopic = roomTopic;
            this.isEncrypted = roomEncrypted;
            this.roomKey = null;
            this.messages = new List<ChatMessage>();
            this.unreadMessages = 0;
            this.isJoined = false;
        }
    }

    public abstract class ChatChannel
    {
        public int unreadMessages;

        public List<ChatRecipient> recipients = new List<ChatRecipient>();

        public List<ChatMessage> messages = new List<ChatMessage>();

        public abstract string DisplayString { get; }

        public abstract bool IsDirect { get; }

        public abstract bool IsRoomChannel(string roomName);

        public abstract bool IsDirectChannel(string directName);

        public ChatMessage AddMessage(ChatMessageType type, string author, string message="")
        {
            ChatMessage msg = new ChatMessage(type, author, message);
            return AddMessage(msg);
        }

        public ChatMessage AddMessage(ChatMessage message)
        {
            messages.Add(message);
            return message;
        }

        public bool ContainsRecipient(string recipient)
        {
            foreach (ChatRecipient recipient2 in recipients)
            {
                if (recipient2.nickname == recipient)
                    return true;
            }
            return false;
        }

        public bool ContainsRecipient(ChatRecipient recipient)
        {
            return ContainsRecipient(recipient.nickname);
        }
    }
}
