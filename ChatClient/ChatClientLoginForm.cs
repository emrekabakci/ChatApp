using System.Data.SqlClient;

namespace ChatUygulamasi.ChatClient
{
    public partial class ChatClientLoginForm : Form
    {
        private ChatClient m_Client;

        public ChatClientLoginForm(ChatClient client)
        {
            InitializeComponent();

            m_Client = client;
            m_Client.Form = null;
            m_Client.Nickname = txtNickname.Text;

            m_Client.OnConnectionSuccess += () =>
            {
                Hide();

                if (Program.chatForm is not null)
                    Program.chatForm.Show();

                m_Client.Channel = null;

                SetLoggingIn(false);
            };

            m_Client.OnError += (string msg) =>
            {
                TaskDialogButton okButton = new TaskDialogButton();
                okButton.Tag = DialogResult.OK;
                okButton.Text = "Tamam";

                TaskDialogPage page = new TaskDialogPage();
                page.Caption = "Hata";
                page.DefaultButton = okButton;
                page.Heading = "Hata";
                page.Icon = TaskDialogIcon.Error;
                page.Text = msg;
                page.Buttons = new TaskDialogButtonCollection() { okButton };

                TaskDialog.ShowDialog(this, page, TaskDialogStartupLocation.CenterScreen);

                SetLoggingIn(false);
            };

            m_Client.OnCertificateValidationFailed += () =>
            {
                SetLoggingIn(false);
            };

            SetLoggingIn(false);

            m_Client.Form = this;
            btnLogin.Focus();
        }

        private void SetLoggingIn(bool login)
        {
            btnLogin.Enabled = !login;
            btnLogin.Text = login ? "Giriş Yapılıyor ..." : "Giriş yap";
        }

        private void Form1_Load(object sender, EventArgs e)
        { }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            if (textBox1.Text == "" || txtNickname.Text == "")
            {
                MessageBox.Show("Lütfen boş alan barakmayınız");
            }
            else
            {
                SqlConnection baglanti = new SqlConnection(SqlCon.ConnectionString);
                baglanti.Open();

                string userName = txtNickname.Text;
                string password = textBox1.Text;
                string sql = string.Format($"select [UserId], [Username], [Pass] from userr where Username = '{userName}' AND Pass = '{password}' ");

                SqlCommand komut = new SqlCommand(sql, baglanti);
                SqlDataReader oku = komut.ExecuteReader();
                if (oku.Read())
                {


                    SetLoggingIn(true);

                    m_Client.Nickname = txtNickname.Text;

                    if (radUseLan.Checked)
                    {
                        m_Client.Hostname = ((string)cboxLan.SelectedValue).Trim();
                    }
                    else
                    {
                        m_Client.Hostname = txtHostname.Text.Trim();
                    }

                    m_Client.Port = 19000;
                    m_Client.Connect();
                }
                else
                {
                    MessageBox.Show("Hatalı kullanıcı adı veya şifre");
                }
            }
        }

        private void ChatClientLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            Program.CheckForExit();
        }

        private void ChatClientLoginForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                m_Client.multicastListener.ServerListChanged = (HashSet<string> serverList) =>
                {
                    if (this.IsDisposed)
                        return;

                    this.Invoke((HashSet<string> serverList) =>
                    {
                        bool hasServers = serverList.Count > 0;

                        if (hasServers)
                        {
                            radUseLan.Enabled = true;
                            cboxLan.Enabled = true;

                            List<string> values = new List<string>(serverList.Count);
                            foreach (string server in serverList)
                                values.Add(server);

                            cboxLan.DataSource = values;
                        }
                        else
                        {
                            radUseHostname.Checked = true;
                            radUseLan.Checked = false;
                            radUseLan.Enabled = false;
                            cboxLan.Enabled = false;
                        }
                    }, serverList);
                };
                m_Client.multicastListener.Start();
            }
            else
            {
                m_Client.multicastListener.ServerListChanged = null;
                m_Client.multicastListener.Stop();
                cboxLan.DataSource = null;
            }
        }

        private void cboxLan_MouseClick(object sender, MouseEventArgs e)
        {
            radUseHostname.Checked = false;
            radUseLan.Checked = true;
        }

        private void txtHostname_MouseClick(object sender, MouseEventArgs e)
        {
            radUseHostname.Checked = true;
            radUseLan.Checked = false;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChatClientSignUp form = new ChatClientSignUp();
            form.Show();
        }
    }
}