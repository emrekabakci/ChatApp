using System.Security.Cryptography.X509Certificates;
using ChatUygulamasi.ChatShared;

namespace ChatUygulamasi.ChatClient
{
    public partial class ChatClientForm : Form
    {
        private ChatClient m_Client;

        public ChatClientForm(ChatClient client)
        {
            InitializeComponent();

            m_Client = client;
            m_Client.Form = this;

            lblHeading.Text = "";
            txtCompose.Enabled = false;
            btnSend.Enabled = false;

            m_Client.Channel = null;

            txtMessages.ScrolledToBottom += (object? sender, EventArgs e) =>
            {
                btnScrollToBottom.Hide();
                btnScrollToBottom.Enabled = false;
            };
            txtMessages.UnscrolledFromBottom += (object? sender, EventArgs e) =>
            {
                btnScrollToBottom.Show();
                btnScrollToBottom.Enabled = true;
            };

            lstChannels.DisplayMember = "DisplayString";
            lstRooms.DisplayMember = "DisplayString";

            m_Client.OnConnectionSuccess += () => { NoConnection(false); };

            m_Client.OnError += (string msg) => { NoConnection(true); };

            m_Client.OnConnectionLost += () => { NoConnection(true, true); };

            m_Client.OnLoginNameChanged += (string name) => { lblMyName.Text = "Hoşgeldin: " + name; };

            m_Client.OnChannelListUpdate += () =>
            {
                RefreshChannelsList();
            };

            Text = Program.AppName;

            m_Client.OnChannelChanged += () =>
            {
                if (m_Client.Channel is null)
                {
                    lblHeading.Text = "";
                    Text = Program.AppName;
                    txtCompose.Enabled = false;
                    btnSend.Enabled = false;
                    txtMessages.Text = "";
                }
                else
                {
                    m_Client.Channel.unreadMessages = 0;

                    RefreshChannelsList();

                    if (m_Client.Channel.IsDirect)
                    {
                        ChatDirectChannel dc = (ChatDirectChannel)m_Client.Channel;
                        lblHeading.Text = dc.Recipient.nickname + ":";
                        Text = $"{Program.AppName} — {dc.Recipient.nickname}";
                    }
                    else
                    {
                        ChatRoomChannel rc = (ChatRoomChannel)m_Client.Channel;
                        lblHeading.Text = rc.roomName + ":";
                        Text = $"{Program.AppName} — {rc.roomName}";
                    }

                    txtCompose.Enabled = true;
                    btnSend.Enabled = true;

                    txtMessages.Text = "";

                    foreach (ChatMessage msg in m_Client.Channel.messages)
                    {
                        txtMessages.Text += msg.ToString() + "\n";
                    }

                    txtCompose.Focus();
                }

                txtMessages.ScrollToBottom();
            };

            m_Client.OnMessageReceived += (ChatChannel channel, ChatMessage msg) =>
            {
                if (m_Client.Channel == channel)
                {
                    bool shouldScroll = txtMessages.IsAtMaxScroll();

                    txtMessages.Text += msg.ToString() + "\n";

                    if (shouldScroll)
                        txtMessages.ScrollToBottom();

                    return true;
                }

                return false;
            };

            m_Client.OnRoomMessageListReceived += (ChatRoomChannel channel) =>
            {
                if (m_Client.Channel == channel)
                {
                    txtMessages.Text = string.Empty;

                    foreach (ChatMessage msg in channel.messages)
                        txtMessages.Text += msg.ToString() + "\n";

                    txtMessages.ScrollToBottom();
                }
            };

            m_Client.OnClientJoin += (ChatRecipient recipient) =>
            {
                RefreshChannelsList();
            };

            m_Client.OnClientLeave += (ChatRecipient recipient) =>
            {
                RefreshChannelsList();
            };

            m_Client.OnCertificateValidationFailed += () =>
            {
                NoConnection(true);
            };

            m_Client.OnCertificateFirstTime += (X509Certificate cert) =>
            {
                TaskDialogButton okButton = new TaskDialogButton();
                okButton.Tag = DialogResult.OK;
                okButton.Text = "OK";

                TaskDialogButton cancelButton = new TaskDialogButton();
                cancelButton.Tag = DialogResult.No;
                cancelButton.Text = "I don't trust this certificate";

                TaskDialogExpander expander = new TaskDialogExpander();
                expander.Expanded = false;
                expander.CollapsedButtonText = "Show Certificate Details";
                expander.ExpandedButtonText = "Hide Certificate Details";
                expander.Text = $"Certificate details:\n" +
                                $"    Subject: {cert.Subject}\n" +
                                $"    Fingerprint: {cert.GetCertHashString()}\n" +
                                $"    Issued by: {cert.Issuer}\n" +
                                $"    Issued: {cert.GetEffectiveDateString()}\n" +
                                $"    Expires: {cert.GetExpirationDateString()}\n";

                TaskDialogPage page = new TaskDialogPage();
                page.Caption = "Security Information";
                page.DefaultButton = okButton;
                page.Expander = expander;
                page.Heading = "This is your first time connecting to this server";
                page.Icon = TaskDialogIcon.Information;
                page.Text = "Click OK if you trust the certificate the server has presented.\n\n" +
                            "You may wish to review the certificate details and " +
                            "determine whether you trust it or not.";
                page.Buttons = new TaskDialogButtonCollection() { okButton, cancelButton };

                if (Program.loginForm is null)
                    return false;

                TaskDialogButton result = TaskDialog.ShowDialog(Program.loginForm, page);

                if (result.Tag is null)
                    return false;

                return (DialogResult)result.Tag == DialogResult.OK;
            };

            m_Client.OnCertificateChanged += (X509Certificate newCert, string oldFingerprint) =>
            {
                TaskDialogButton yesButton = new TaskDialogButton();
                yesButton.Tag = DialogResult.Yes;
                yesButton.Text = "Trust the new certificate";

                TaskDialogButton noButton = new TaskDialogButton();
                noButton.Tag = DialogResult.No;
                noButton.Text = "Reject the new certificate";

                TaskDialogExpander expander = new TaskDialogExpander();
                expander.Expanded = false;
                expander.CollapsedButtonText = "Review Certificate Details";
                expander.ExpandedButtonText = "Hide Certificate Details";
                expander.Text = $"Trusted certificate details:\n" +
                                $"    Fingerprint: {oldFingerprint}\n" +
                                $"\n" +
                                $"New Certificate details:\n" +
                                $"    Fingerprint: {newCert.GetCertHashString()}\n" +
                                $"    Subject: {newCert.Subject}\n" +
                                $"    Issued by: {newCert.Issuer}\n" +
                                $"    Issued: {newCert.GetEffectiveDateString()}\n" +
                                $"    Expires: {newCert.GetExpirationDateString()}\n";

                TaskDialogPage page = new TaskDialogPage();
                page.Caption = "Security Warning";
                page.DefaultButton = noButton;
                page.Expander = expander;
                page.Heading = "Server sent an unknown certificate";
                page.Icon = TaskDialogIcon.ShieldErrorRedBar;
                page.Text = "Do you want trust the new certificate the server has presented?\n\n" +
                            "This could be due to a man-in-the-middle attack, or more likely, " +
                            "the certificate on the server may have been updated by the server's administrators.\n\n" +
                            "Please review the certificate details below and " +
                            "determine whether you wish to trust the new certificate or not.\n\n";
                page.Buttons = new TaskDialogButtonCollection() { yesButton, noButton };
                page.AllowCancel = false;

                if (Program.loginForm is null)
                    return false;

                TaskDialogButton result = TaskDialog.ShowDialog(Program.loginForm, page);

                if (result.Tag is null)
                    return false;

                return (DialogResult)result.Tag == DialogResult.Yes;
            };

            m_Client.OnRoomPasswordRequested += () =>
            {
                if (Program.roomPasswordForm is null)
                    return null;

                Program.roomPasswordForm.Reset();
                if (Program.roomPasswordForm.ShowDialog(this) == DialogResult.OK)
                {
                    return Program.roomPasswordForm.GetPassword();
                }

                return null;
            };

            txtCompose.Focus();
        }

        private void NoConnection(bool notConnected, bool inform = false)
        {
            lblServer.Text = $"Server: {m_Client.Hostname}:{m_Client.Port}";

            btnConnect.Enabled = notConnected;
            btnConnect.Text = notConnected ? "Yeniden Bağlan" : "Bağlandı";

            if (notConnected && inform)
            {
                MessageBox.Show("Server ile olan bağlantı kayboldu.  " +
                                "Yeniden bağlanmak için lütfen 'Tekrar Bağlan' butonuna basınız.",
                                "Bağlantı kayboldu.",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void RefreshChannelsList()
        {
            lstChannels.Items.Clear();
            lstRooms.Items.Clear();
            foreach (ChatChannel channel in m_Client.Channels)
            {
                if (channel.IsDirect)
                    lstChannels.Items.Add(channel);
                else
                    lstRooms.Items.Add(channel);
            }

            lstChannels.NoEvents = true;
            lstRooms.NoEvents = true;
            if (m_Client.Channel is null)
            {
                lstChannels.SelectedItem = null;
                lstRooms.SelectedItem = null;
            }
            else
            {
                if (m_Client.Channel.IsDirect)
                    lstChannels.SelectedItem = m_Client.Channel;
                else
                    lstRooms.SelectedItem = m_Client.Channel;
            }
            lstChannels.NoEvents = false;
            lstRooms.NoEvents = false;
        }

        private void ChatClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            Program.CheckForExit();
        }

        private void lstChannels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstChannels.SelectedItem is null)
            {
                m_Client.Channel = null;
            }
            else
            {
                ChatChannel channel = (ChatChannel)lstChannels.SelectedItem;
                m_Client.Channel = channel;
            }
        }

        private void lstRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstRooms.SelectedItem is null)
            {
                m_Client.Channel = null;
            }
            else
            {
                ChatChannel channel = (ChatChannel)lstRooms.SelectedItem;
                m_Client.Channel = channel;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            txtCompose.Focus();

            if (txtCompose.TextLength <= 0)
                return;

            m_Client.SendMessage(txtCompose.Text);

            txtCompose.Clear();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Text = "Bağlanıyor...";
            btnConnect.Enabled = false;
            m_Client.Connect();
        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Client.Disconnect();

            Hide();
            if (Program.loginForm is not null)
                Program.loginForm.Show();

            Program.CheckForExit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnScrollToBottom_Click(object sender, EventArgs e)
        {
            txtMessages.ScrollToBottom();
        }

        private void createRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChatClientRoomCreateForm createForm = new ChatClientRoomCreateForm(m_Client);
            createForm.Show();
        }

        private void roomToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            deleteRoomToolStripMenuItem.Enabled = m_Client.OwnedRooms.Count > 0;

            deleteRoomToolStripMenuItem.DropDownItems.Clear();
            foreach (ChatRoomChannel room in m_Client.OwnedRooms)
            {
                ToolStripButton button = new ToolStripButton(room.roomName);

                button.Text = room.roomName;

                button.Click += (object? sender, EventArgs e) =>
                {
                    if (sender is null)
                        return;

                    string roomName = ((ToolStripButton)sender).Text;

                    TaskDialogButton yesButton = new TaskDialogButton();
                    yesButton.Tag = DialogResult.Yes;
                    yesButton.Text = "Evet";

                    TaskDialogButton noButton = new TaskDialogButton();
                    noButton.Tag = DialogResult.No;
                    noButton.Text = "Hayır";

                    TaskDialogPage page = new TaskDialogPage();
                    page.Caption = "Doğrulama";
                    page.DefaultButton = noButton;
                    page.Heading = "Bu odayi silmek istedigine emin misin?";
                    page.Icon = TaskDialogIcon.Warning;
                    page.Text = "Evete basmak '" + roomName + "'odasını silecektir'.\n\n" +
                                "Odadaki bütün mesaj geçmişi de silinecektir.\n\n" +
                                "Bu işlem geri alınamaz.";
                    page.Buttons = new TaskDialogButtonCollection() { yesButton, noButton };
                    page.AllowCancel = false;

                    TaskDialogButton result = TaskDialog.ShowDialog(this, page);

                    if (result.Tag is null || (DialogResult)result.Tag != DialogResult.Yes)
                        return;

                    m_Client.DeleteRoom(roomName);
                };

                deleteRoomToolStripMenuItem.DropDownItems.Add(button);
            }
        }

        private void ChatClientForm_Resize(object sender, EventArgs e)
        {
            if (txtMessages.IsAtMaxScroll())
            {
                btnScrollToBottom.Hide();
                btnScrollToBottom.Enabled = false;
            }
            else
            {
                btnScrollToBottom.Show();
                btnScrollToBottom.Enabled = true;
            }
        }

        private int StringLastWordIndex(string s, int caret)
        {
            if (caret < 1)
                return -1;

            int i;
            for (i = caret - 1; i > 0; --i)
            {
                char c = s[i];

                if (!char.IsWhiteSpace(c))
                    break;
            }

            char lastChar = s[i];
            for (; i > 0; --i)
            {
                if (char.IsPunctuation(lastChar) && !char.IsPunctuation(s[i - 1]))
                    return i;

                if (!char.IsPunctuation(lastChar) && char.IsPunctuation(s[i - 1]))
                    return i;

                if (char.IsWhiteSpace(s[i - 1]))
                    return i;
            }

            return 0;
        }

        private void txtCompose_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompose.SelectionLength == 0 &&
                ((e.Shift && e.KeyCode == Keys.Back) ||
                (e.Control && e.KeyCode == Keys.W)))
            {
                int index = StringLastWordIndex(txtCompose.Text, txtCompose.SelectionStart);

                if (index > -1)
                {
                    string oldText = txtCompose.Text;
                    string lside = oldText.Substring(0, index);
                    string rside = string.Empty;

                    if (txtCompose.SelectionStart < txtCompose.Text.Length)
                    {
                        int start = txtCompose.SelectionStart;
                        int end = txtCompose.TextLength;
                        int length = end - start;
                        rside = oldText.Substring(start, length);
                    }

                    txtCompose.Text = lside + rside;
                    txtCompose.SelectionStart = index;
                }
            }
        }

        private void txtMessages_TextChanged(object sender, EventArgs e)
        {

        }
    }
}