using System.Text;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatClient
{
    public enum ChatClientPasswordAwaitState
    {
        None,

        Waiting,

        Incorrect,

        Successful,

        UnknownError,
    };

    public class ChatClient
    {
        private string m_Nickname = string.Empty;

        public string Nickname
        {
            get => m_Nickname;
            set
            {
                m_Nickname = value;

                Invoke(OnLoginNameChanged, m_Nickname);
            }
        }

        private string m_Hostname = string.Empty;

        public string Hostname { get => m_Hostname; set => m_Hostname = value; }

        private int m_Port;

        public int Port { get => m_Port; set => m_Port = value; }

        private TcpClient? m_Tcp;
        private Stream? m_Stream;
        private BinaryReader? m_Reader;
        private BinaryWriter? m_Writer;

        public BinaryReader Reader
        {
            get
            {
                if (m_Reader is null)
                    throw new NullReferenceException();

                return m_Reader;
            }
        }

        public BinaryWriter Writer
        {
            get
            {
                if (m_Writer is null)
                    throw new NullReferenceException();

                return m_Writer;
            }
        }

        private ChatClientPacketHandler m_PacketHandler;

        public ChatClientMulticastListener multicastListener;

        private List<ChatChannel> m_Channels = new List<ChatChannel>();

        public List<ChatChannel> Channels { get => m_Channels; }

        private List<ChatRoomChannel> m_OwnedRooms = new List<ChatRoomChannel>();

        public List<ChatRoomChannel> OwnedRooms { get => m_OwnedRooms; }

        private Dictionary<string, byte[]?> m_RoomKeychain = new Dictionary<string, byte[]?>();

        public Dictionary<string, byte[]?> RoomKeychain { get => m_RoomKeychain; }

        private static readonly int RoomPasswordResponseTimeout = 5;

        private Dictionary<string, ChatRecipient> m_Clients = new Dictionary<string, ChatRecipient>();

        public Dictionary<string, ChatRecipient> Clients { get => m_Clients; }

        private ChatChannel? m_Channel = null;

        public ChatChannel? Channel
        {
            get => m_Channel;
            set
            {
                if (value is not null)
                {
                    if (!m_Channels.Contains(value))
                    {
                        Invoke(() => MessageBox.Show($"Unknown channel"));
                        return;
                    }
                }

                if (value is not null &&
                    !value.IsDirect &&
                    !((ChatRoomChannel)value).isJoined &&
                    !JoinRoom((ChatRoomChannel)value))
                {
                }
                else
                {
                    m_Channel = value;
                }

                Invoke(OnChannelChanged);
            }
        }

        public Action? OnConnectionSuccess;

        public Action? OnConnectionLost;
        public Action<string>? OnLoginNameChanged;
        public Action<string>? OnError;
        public Action? OnChannelListUpdate;
        public Action? OnChannelChanged;
        public Func<ChatChannel, ChatMessage, bool>? OnMessageReceived;
        public Action<ChatRoomChannel>? OnRoomMessageListReceived;
        public Action<ChatRecipient>? OnClientLeave;
        public Action<ChatRecipient>? OnClientJoin;
        public Func<X509Certificate, string, bool>? OnCertificateChanged;
        public Func<X509Certificate, bool>? OnCertificateFirstTime;
        public Action? OnCertificateValidationFailed;
        public Action? OnRoomCreateSuccess;
        public Action<string>? OnRoomCreateFail;
        public Action<string>? OnRoomDeleteFail;
        public Func<string?>? OnRoomPasswordRequested;
        public Action? OnRoomPasswordPending;
        public Action? OnRoomPasswordResponse;
        public Action<string>? OnRoomPasswordError;
        public Action? OnRoomPasswordCorrect;

        private bool m_InServer = false;

        public bool InServer
        {
            get
            {
                lock (threadStopSync)
                    return m_InServer;
            }
            set
            {
                lock (threadStopSync)
                    m_InServer = value;
            }
        }

        private bool m_StopThread = false;

        private bool StopThread
        {
            get
            {
                lock (threadStopSync)
                    return m_StopThread;
            }
            set
            {
                lock (threadStopSync)
                    m_StopThread = value;
            }
        }

        private Thread? m_Thread;

        private Form? m_Form = null;

        public Form Form
        {
            get
            {
                if (m_Form is null)
                    throw new NullReferenceException();

                return m_Form;
            }
            set => m_Form = value;
        }

        public ChatClientPasswordAwaitState passwordAwait = ChatClientPasswordAwaitState.None;

        public readonly object threadStopSync = new object();

        public readonly object passwordAwaitSync = new object();

        public override string ToString() => Nickname;

        private object Invoke(Delegate? method, params object[] args)
        {
            if (m_Form is null || method is null)
                return false;

            return m_Form.Invoke(method, args);
        }

        public ChatClient(string nickname, int port)
        {
            m_Form = null;
            Nickname = nickname;
            Port = port;
            m_Channel = null;

            m_PacketHandler = new ChatClientPacketHandler(this);

            multicastListener = new ChatClientMulticastListener();

            StopThread = false;
            InServer = false;

            lock (passwordAwaitSync)
                passwordAwait = ChatClientPasswordAwaitState.None;
        }

        public void SendMessage(string message)
        {
            if (m_Writer is null)
                return;

            if (Channel is null)
                return;

            bool isEncrypted = false;
            byte[]? iv = null;

            if (Channel.IsDirect)
            {
                ChatDirectChannel dc = (ChatDirectChannel)Channel;

                using (Packet packet = new Packet(PacketType.ClientDirectMessage))
                {
                    packet.Write(dc.Recipient.nickname);

                    packet.Write(message);

                    packet.WriteToStream(Writer);
                    Writer.Flush();
                }
            }
            else
            {
                ChatRoomChannel rc = (ChatRoomChannel)Channel;

                if (rc.isEncrypted)
                {
                    if (!RoomKeychain.ContainsKey(rc.roomName))
                    {
                        ShowError($"No key for room '{rc.roomName}'");
                        return;
                    }

                    byte[]? roomKey = RoomKeychain[rc.roomName];
                    if (roomKey is null)
                    {
                        ShowError($"No key for room '{rc.roomName}'");
                        return;
                    }

                    isEncrypted = true;

                    using (Aes aes = Aes.Create())
                    {
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.BlockSize = 128;  
                        aes.Key = roomKey;
                        iv = aes.IV;

                        ICryptoTransform encryptor = aes.CreateEncryptor();

                        byte[] cipherMessage;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            {
                                using (StreamWriter sw = new StreamWriter(cs))
                                {
                                    sw.Write(message);
                                }
                                cipherMessage = ms.ToArray();
                            }
                        }

                        message = Convert.ToBase64String(cipherMessage);
                    }
                }

                using (Packet packet = new Packet(PacketType.ClientRoomMessage))
                {
                    packet.Write(rc.roomName);

                    packet.Write(message);

                    if (isEncrypted && iv is not null)
                        packet.Write(Convert.ToBase64String(iv));

                    packet.WriteToStream(Writer);
                    Writer.Flush();
                }
            }
        }

        public void CreateRoom(string roomName, string roomTopic, bool roomEncrypted, string roomPassword = "")
        {
            foreach (ChatChannel channel in Channels)
            {
                if (channel.IsRoomChannel(roomName))
                {
                    Invoke(OnRoomCreateFail, "Lütfen farklı bir oda ismi seçin.");
                    return;
                }
            }

            if (roomEncrypted)
            {
                if (string.IsNullOrEmpty(roomPassword))
                {
                    Invoke(OnRoomCreateFail, "Lütfen bir oda şifresi yazın.");
                    return;
                }

                byte[] key = ChatClientCrypto.DeriveKey(roomPassword, ChatClientCrypto.Salt);

                if (RoomKeychain.ContainsKey(roomName))
                    RoomKeychain[roomName] = key;
                else
                    RoomKeychain.Add(roomName, key);
            }

            using (Packet packet = new Packet(PacketType.ClientRoomCreate))
            {
                packet.Write(roomName);

                packet.Write(roomTopic);

                packet.Write(roomEncrypted);

                packet.WriteToStream(Writer);
                Writer.Flush();
            }
        }

        public void DeleteRoom(string roomName)
        {
            using (Packet packet = new Packet(PacketType.ClientRoomDelete))
            {
                packet.Write(roomName);

                packet.WriteToStream(Writer);
                Writer.Flush();
            }
        }

        private bool JoinRoomEncrypted(ChatRoomChannel room)
        {
            if (OnRoomPasswordRequested is null ||
                OnRoomPasswordPending is null)
            {
                return false;
            }

            while (true)
            {
                lock (passwordAwaitSync)
                    passwordAwait = ChatClientPasswordAwaitState.None;

                string? password = (string?)Invoke(OnRoomPasswordRequested);
                if (password is null)
                {
                    return false;
                }

                byte[] key = ChatClientCrypto.DeriveKey(password, ChatClientCrypto.Salt);

                if (RoomKeychain.ContainsKey(room.roomName))
                    RoomKeychain[room.roomName] = key;
                else
                    RoomKeychain.Add(room.roomName, key);

                string plainText = Nickname + room.roomName + ChatClientCrypto.SaltString;

                CryptoCipher cipher = ChatClientCrypto.EncryptMessage(plainText, key);

                using (Packet packet = new Packet(PacketType.ClientRoomJoin))
                {
                    packet.Write(room.roomName);

                    packet.Write(ChatClientCrypto.SaltString);

                    packet.Write(cipher.IVString);

                    packet.Write(cipher.CipherString);

                    packet.WriteToStream(Writer);
                    Writer.Flush();
                }

                Invoke(OnRoomPasswordPending);

                lock (passwordAwaitSync)
                    passwordAwait = ChatClientPasswordAwaitState.Waiting;

                DateTime startTime = DateTime.Now;
                for (; ; )
                {
                    lock (passwordAwaitSync)
                    {
                        if (passwordAwait != ChatClientPasswordAwaitState.Waiting)
                            break;
                    }

                    Thread.Sleep(100);

                    DateTime now = DateTime.Now;
                    if ((now - startTime).TotalSeconds > RoomPasswordResponseTimeout)
                    {
                        ShowError("Server took too long to respond to password input.");
                        break;
                    }
                }

                Invoke(OnRoomPasswordResponse);

                lock (passwordAwaitSync)
                {
                    switch (passwordAwait)
                    {
                        case ChatClientPasswordAwaitState.Incorrect:
                            if (RoomKeychain.ContainsKey(room.roomName))
                                RoomKeychain[room.roomName] = null;

                            Invoke(OnRoomPasswordError, "The password is incorrect.");
                            break;

                        case ChatClientPasswordAwaitState.UnknownError:
                            if (RoomKeychain.ContainsKey(room.roomName))
                                RoomKeychain[room.roomName] = null;

                            Invoke(OnRoomPasswordError, "An internal error occurred while validating the password.");
                            break;

                        case ChatClientPasswordAwaitState.Successful:
                            Invoke(OnRoomPasswordCorrect);
                            return true;

                        default: break;
                    }
                }
            }
        }

        public bool JoinRoom(ChatRoomChannel room)
        {
            if (room.isEncrypted)
                return JoinRoomEncrypted(room);

            using (Packet packet = new Packet(PacketType.ClientRoomJoin))
            {
                packet.Write(room.roomName);

                packet.WriteToStream(Writer);
                Writer.Flush();
            }

            return true;
        }

        private void Cleanup()
        {
            if (m_Writer is not null)
            {
                m_Writer.Close();
                m_Writer = null;
            }

            if (m_Reader is not null)
            {
                m_Reader.Close();
                m_Reader = null;
            }

            if (m_Stream is not null)
            {
                m_Stream.Close();
                m_Stream = null;
            }

            if (m_Tcp is not null)
            {
                m_Tcp.Close();
                m_Tcp = null;
            }

            if (m_Thread is not null)
            {
                StopThread = true;

            }

            multicastListener.Stop();

            InServer = false;
        }

        public void Disconnect()
        {
            InServer = false;
            StopThread = true;

            if (m_Writer is not null)
            {
                using (Packet packet = new Packet(PacketType.ClientDisconnect))
                {
                    packet.WriteToStream(Writer);
                    Writer.Flush();
                }
            }

            Cleanup();
        }

        private void ShowError(string msg) => Invoke(OnError, msg);

        public void Connect()
        {
            StopThread = false;

            m_Thread = new Thread(new ThreadStart(Run));
            m_Thread.IsBackground = true;
            m_Thread.Start();
        }

        private bool ValidateCertificate(object? sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors policyErrors)
        {
            if (cert is null)
            {
                ShowError("Server did not present a certificate.");
                return false;
            }

            Dictionary<string, string> trustedCertificates = new Dictionary<string, string>();

            using (FileStream file = new FileStream(Program.TOFUPath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                if (!file.CanRead)
                {
                    ShowError("Failed to open TOFU file for reading.");
                    return false;
                }

                using (StreamReader reader = new StreamReader(file))
                {
                    for (string? line = reader.ReadLine();
                         line is not null;
                         line = reader.ReadLine())
                    {
                        string[] columns = line.Split(" ");

                        if (columns.Length != 2)
                            continue;

                        trustedCertificates.Add(columns[0], columns[1]);
                    }
                }

                string certName = $"{Hostname}:{Port}";
                string certFingerprint = cert.GetCertHashString();

                if (!trustedCertificates.ContainsKey(certName))
                {
                    trustedCertificates.Add(certName, certFingerprint);

                    if (!(bool)Invoke(OnCertificateFirstTime, cert))
                    {
                        return false;
                    }
                }
                else
                {
                    if (trustedCertificates[certName] != certFingerprint)
                    {
                        if (!(bool)Invoke(OnCertificateChanged,
                                          cert,
                                          trustedCertificates[certName]))
                        {
                            return false;
                        }

                        trustedCertificates[certName] = certFingerprint;
                    }
                }
            }

            using (FileStream file = new FileStream(Program.TOFUPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (!file.CanWrite)
                {
                    ShowError("Failed to open TOFU file for writing.");
                    return false;
                }

                using (StreamWriter writer = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, string> certificate in trustedCertificates)
                    {
                        writer.WriteLine($"{certificate.Key} {certificate.Value}");
                    }
                }
            }

            return true;
        }

        public ChatDirectChannel? GetDirectChannelForClient(ChatRecipient recipient)
        {
            foreach (ChatChannel channel2 in Channels)
            {
                if (channel2.IsDirectChannel(recipient.nickname))
                    return (ChatDirectChannel)channel2;
            }

            return null;
        }

        public ChatDirectChannel? GetDirectChannelForClient(string nickname)
        {
            if (!m_Clients.ContainsKey(nickname))
                return null;

            return GetDirectChannelForClient(m_Clients[nickname]);
        }

        private void Run()
        {
            try
            {
                m_Tcp = new TcpClient(Hostname, Port);
            }
            catch (Exception e)
            {
                ShowError(e.Message);
                return;
            }

            m_Stream = m_Tcp.GetStream();

            SslClientAuthenticationOptions options = new();
            options.TargetHost = Hostname;

            m_Stream.ReadTimeout = Timeout.Infinite;
            m_Stream.WriteTimeout = Timeout.Infinite;

            m_Writer = new BinaryWriter(m_Stream, Encoding.UTF8);
            m_Reader = new BinaryReader(m_Stream, Encoding.UTF8);

            m_Clients.Clear();
            m_Channels.Clear();
            m_OwnedRooms.Clear();
            m_RoomKeychain.Clear();

            using (Packet packet = new Packet(PacketType.ClientHello))
            {
                packet.Write(Nickname);
                packet.WriteToStream(m_Writer);
                m_Writer.Flush();
            }

            InServer = false;

            while (true)
            {
                if (StopThread)
                    break;

                try
                {
                    if (!m_Tcp.Connected)
                        break;

                    using (Packet packet = new Packet(Reader))
                    {
                        if (!InServer)
                        {
                            if (packet.PacketType == PacketType.ServerError)
                            {
                                PacketErrorCode id = (PacketErrorCode)packet.ReadUInt32();
                                string msg = packet.ReadString();
                                ShowError($"Server error {id}: {msg}");
                                Cleanup();
                                return;
                            }

                            if (packet.PacketType != PacketType.ServerWelcome)
                            {
                                ShowError("Error: unexpected packet.");
                                Cleanup();
                                return;
                            }
                        }

                        m_PacketHandler.Handle(packet);
                    }
                }
                catch (IOException)
                {
                    if (InServer)
                    {
                        Invoke(OnConnectionLost);
                    }
                    InServer = false;
                    Cleanup();
                    return;
                }
                catch (ObjectDisposedException)
                {
                    if (InServer)
                    {
                        Invoke(OnConnectionLost);
                    }
                    InServer = false;
                    Cleanup();
                    return;
                }
                catch (Exception e)
                {
                    if (InServer)
                    {
                        Invoke(OnConnectionLost);
                    }
                    InServer = false;
                    Cleanup();

                    ShowError($"Exception: {e.Message}");
                    return;
                }
            }
        }
    }
}