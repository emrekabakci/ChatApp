using System.Text;

namespace ChatUygulamasi.ChatShared
{
    public enum PacketType : uint
    {
        ClientHello,

        ClientDisconnect,

        ClientDirectMessage,

        ClientRoomMessage,

        ClientRoomJoin,

        ClientRoomLeave,

        ClientRoomCreate,

        ClientRoomDelete,

        ClientEncryptedRoomAuthorise,

        ClientEncryptedRoomAuthoriseFail,

        ServerError,

        ServerWelcome,

        ServerClientList,

        ServerRoomList,

        ServerClientJoin,

        ServerClientLeave,

        ServerDirectMessageReceived,

        ServerRoomMessageReceived,

        ServerClientRoomJoin,

        ServerClientRoomLeave,

        ServerClientRoomMembers,

        ServerClientRoomMessages,

        ServerClientJoinEncryptedRoomRequest,

        ServerClientEncryptedRoomAuthorise,

        ServerClientEncryptedRoomAuthoriseFail,

        ServerRoomCreated,

        ServerRoomDeleted,

        ServerRoomCreateError,

        ServerRoomDeleteError,

        ServerRoomOwnerChange,
    }

    public enum PacketErrorCode : uint
    {
        OK,

        UnknownError,

        EnteredUser,

        RoomNameTaken,

        PasswordMismatch,
    }

    public class Packet : IDisposable
    {
        private int m_Position;
        private List<byte>? m_Buffer;
        private byte[]? m_ReadableBuffer;

        private bool m_Disposed = false;

        private bool m_IsLocked = false;

        public void Lock() => m_IsLocked = false;
        public void Unlock() => m_IsLocked = false;

        public int Length 
        {
            get 
            {
                if (m_Buffer is null)
                    throw new NullReferenceException();

                return m_Buffer.Count; 
            }
        }
        public int UnreadLength { get => Length - m_Position; }

        public PacketType PacketType 
        {
            get 
            {
                if (m_ReadableBuffer is null)
                    throw new NullReferenceException();

                return (PacketType)BitConverter.ToUInt32(m_ReadableBuffer, 0);
            }
        }

        public Packet()
        {
            m_Buffer = new List<byte>();
            m_Position = 0;
        }

        public Packet(PacketType packetType)
        {
            m_Buffer = new List<byte>();
            m_Position = 0;

            Write((uint)packetType);
        }

        public Packet(byte[] data)
        {
            m_Buffer = new List<byte>();
            m_Position = 0;

            SetBytes(data);
        }

        public Packet(BinaryReader br)
        {
            ReadFromStream(br);
        }

        public void SetBytes(byte[] data)
        {
            Write(data);
            m_ReadableBuffer = m_Buffer?.ToArray();

            m_Position = sizeof(int);
        }

        public void Write(byte data)
        {

            m_Buffer?.Add(data);
        }

        public void Write(byte[] data)
        {
            if (m_IsLocked)
                throw new Exception("Packet is locked for writing.");

            m_Buffer?.AddRange(data);
        }

        public void Write(bool data)
        {
            if (m_IsLocked)
                throw new Exception("Packet is locked for writing.");

            m_Buffer?.AddRange(BitConverter.GetBytes(data));
        }

        public void Write(uint data)
        {
            if (m_IsLocked)
                throw new Exception("Packet is locked for writing.");

            m_Buffer?.AddRange(BitConverter.GetBytes(data));
        }

        public void Write(int data) 
        { 
            if (m_IsLocked)
                throw new Exception("Packet is locked for writing.");

            m_Buffer?.AddRange(BitConverter.GetBytes(data)); 
        }

        public void Write(float data) 
        { 
            if (m_IsLocked)
                throw new Exception("Packet is locked for writing.");

            m_Buffer?.AddRange(BitConverter.GetBytes(data)); 
        }

        public void Write(string data)
        {
            if (m_IsLocked)
                throw new Exception("Packet is locked for writing.");

            Write(data.Length);
            m_Buffer?.AddRange(Encoding.UTF8.GetBytes(data));
        }

        public byte ReadByte(bool updatePosition=true)
        {
            if (m_Buffer is null || m_ReadableBuffer is null)
                throw new NullReferenceException();

            if (m_Buffer.Count <= m_Position)
                throw new EndOfStreamException();

            byte value = m_ReadableBuffer[m_Position];

            if (updatePosition)
            {
                m_Position += sizeof(byte);
            }

            return value;
        }

        public byte[] ReadBytes(int count, bool updatePosition=true)
        {
            if (m_Buffer is null)
                throw new NullReferenceException();

            if (m_Buffer.Count <= m_Position)
                throw new EndOfStreamException();

            byte[] value = m_Buffer.GetRange(m_Position, count).ToArray();

            if (updatePosition)
            {
                m_Position += count * sizeof(byte);
            }

            return value;
        }

        public bool ReadBool(bool updatePosition=true)
        {
            if (m_Buffer is null || m_ReadableBuffer is null)
                throw new NullReferenceException();

            if (m_Buffer.Count <= m_Position)
                throw new EndOfStreamException();

            bool value = BitConverter.ToBoolean(m_ReadableBuffer, m_Position);

            if (updatePosition)
            {
                m_Position += sizeof(bool);
            }

            return value;
        }

        public int ReadInt32(bool updatePosition=true)
        {
            if (m_ReadableBuffer is null)
                throw new NullReferenceException();

            if (m_Buffer?.Count <= m_Position)
                throw new EndOfStreamException();

            int value = BitConverter.ToInt32(m_ReadableBuffer, m_Position);

            if (updatePosition)
            {
                m_Position += sizeof(int);
            }

            return value;
        }

        public uint ReadUInt32(bool updatePosition=true)
        {
            if (m_ReadableBuffer is null)
                throw new NullReferenceException();

            if (m_Buffer?.Count <= m_Position)
                throw new EndOfStreamException();

            uint value = BitConverter.ToUInt32(m_ReadableBuffer, m_Position);

            if (updatePosition)
            {
                m_Position += sizeof(uint);
            }

            return value;
        }

        public string ReadString(bool updatePosition=true)
        {
            if (m_ReadableBuffer is null)
                throw new NullReferenceException();

            try
            {
                int length = ReadInt32();

                string value = Encoding.UTF8.GetString(m_ReadableBuffer, m_Position, length);

                if (updatePosition && value.Length > 0)
                {
                    m_Position += length;
                }

                return value;
            }
            catch 
            {
                throw new Exception("Failed to read string");
            }
        }

        public void WriteToStream(BinaryWriter bw)
        {
            if (m_Buffer is null)
                return;

            m_ReadableBuffer = m_Buffer.ToArray();

            bw.Write(Length);

            bw.Write(m_ReadableBuffer);
        }

        public void ReadFromStream(BinaryReader br)
        {
            m_Buffer = new List<byte>();
            m_Position = 0;

            int length = br.ReadInt32();

            SetBytes(br.ReadBytes(length));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
            {
                m_Buffer = null;
                m_ReadableBuffer = null;
                m_Position = 0;
            }

            m_Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
