using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatClient
{
    internal static class Program
    {
        public static ChatClient? client;

        public static ChatClientLoginForm? loginForm;

        public static ChatClientForm? chatForm;

        public static ChatClientRoomPasswordForm? roomPasswordForm;

        public static List<ChatClientRoomCreateForm> roomCreateForms = new List<ChatClientRoomCreateForm>();

        public static readonly string AppName = "ChatApp";

        public static readonly string TOFUPath = "tofu.txt";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            client = new ChatClient("", ChatConstants.ServerPort);
            chatForm = new ChatClientForm(client);
            loginForm = new ChatClientLoginForm(client);
            roomPasswordForm = new ChatClientRoomPasswordForm(client);

            loginForm.Hide();
            chatForm.Hide();
            roomPasswordForm.Hide();

            Application.Run(loginForm);
        }

        private static void Cleanup()
        {
            foreach (ChatClientRoomCreateForm form in roomCreateForms)
                form.Close();

            if (roomPasswordForm is not null)
                roomPasswordForm.Close();

            if (client is not null)
                client.Disconnect();
        }

        public static void CheckForExit()
        {
            bool allHidden = loginForm != null && !loginForm.Visible &&
                             chatForm != null && !chatForm.Visible;

            if (allHidden)
            {
                Cleanup();

                Application.Exit();
            }
        }
    }
}