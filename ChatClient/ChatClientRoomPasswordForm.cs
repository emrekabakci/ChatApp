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
    public partial class ChatClientRoomPasswordForm : Form
    {
        private ChatClient m_Client;

        public ChatClientRoomPasswordForm(ChatClient client)
        {
            InitializeComponent();

            m_Client = client;

            m_Client.OnRoomPasswordPending += () =>
            {
                btnOk.Enabled = false;
            };

            m_Client.OnRoomPasswordResponse += () => { btnOk.Enabled = true; };

            m_Client.OnRoomPasswordError += (string message) =>
            {
                MessageBox.Show(message, "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

        }

        private void ChatClientRoomPasswordForm_Shown(object sender, EventArgs e)
        {
            Reset();
        }

        public string? GetPassword()
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
                return null;

            return txtPassword.Text;
        }

        private void CheckLength()
        {
            btnOk.Enabled = txtPassword.TextLength > 0;
        }

        public void Reset()
        {
            txtPassword.ResetText();
            CheckLength();
            txtPassword.Focus();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            CheckLength();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
