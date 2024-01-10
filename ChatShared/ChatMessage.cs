namespace ChatUygulamasi.ChatShared
{
    public enum ChatMessageType
    { 
        UserMessage,
        UserJoin,
        UserLeave,
        UserJoinRoom,
        UserLeaveRoom,

        RoomCreated,
        RoomOwnerChanged,
        RoomTopicSet,
    }

    public class ChatMessage
    {
        public ChatMessageType type;

        public string author;

        public string? message;

        public string? ivString;

        public ChatMessage(ChatMessageType type, 
                           string author, 
                           string? message=null, 
                           string? ivString=null)
        {
            this.type = type;
            this.author = author;
            this.message = message;
            this.ivString = ivString;
        }

        public override string ToString()
        {
            switch (type)
            {
                case ChatMessageType.UserMessage:
                    return $"<{author}>: {message}";
                case ChatMessageType.UserLeave:
                    return $"{author} disconnected.";
                case ChatMessageType.UserJoin:
                    return $"{author} connected.";
                case ChatMessageType.UserLeaveRoom:
                    return $"{author} left the room.";
                case ChatMessageType.UserJoinRoom:
                    return $"{author} joined the room.";
                case ChatMessageType.RoomCreated:
                    return $"{author} created the room.";
                case ChatMessageType.RoomOwnerChanged:
                    return $"Room ownership transferred to {author}";
                case ChatMessageType.RoomTopicSet:
                    if (message is not null)
                        return $"{author} set the room topic to {message}";
                    return $"{author} removed the room topic.";
                default:
                    return string.Empty;
            }
        }
    }
}
