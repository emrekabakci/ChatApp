using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatUygulamasi.ChatClient
{
    public partial class ChatClientRoomCreateForm : Form
    {
        private ChatClient m_Client;

        public ChatClientRoomCreateForm(ChatClient client)
        {
            InitializeComponent();

            m_Client = client;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            btnCreate.Enabled = false;
            btnCreate.Text = "Oluşturuluyor ...";

            m_Client.OnRoomCreateSuccess = () => { Close(); };
            m_Client.OnRoomCreateFail = (string error) =>
            {
                MessageBox.Show("Oda oluşturulamadı: " + error, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                btnCreate.Text = "Oluştur";
                btnCreate.Enabled = true;
            };
            string empty = "";
            m_Client.CreateRoom(roomName: txtName.Text,
                                roomTopic: "",
                                roomEncrypted: chkEncrypt.Checked,
                                roomPassword: txtPassword.Text);
        }

        private void UpdateCreateButtonEnabledState()
        {
            btnCreate.Enabled = txtName.TextLength > 0;

            if (chkEncrypt.Checked && txtPassword.TextLength <= 0)
            {
                btnCreate.Enabled = false;
            }
        }

        private void chkEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.Enabled =
            lblPassword.Enabled = chkEncrypt.Checked;
            UpdateCreateButtonEnabledState();
        }

        private void txtName_TextChanged(object sender, EventArgs e) => UpdateCreateButtonEnabledState();
        private void txtPassword_TextChanged(object sender, EventArgs e) => UpdateCreateButtonEnabledState();
    }
}
