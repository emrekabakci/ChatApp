namespace ChatUygulamasi.ChatClient
{
    partial class ChatClientRoomCreateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            label1 = new Label();
            txtName = new TextBox();
            groupBox2 = new GroupBox();
            txtPassword = new TextBox();
            lblPassword = new Label();
            chkEncrypt = new CheckBox();
            btnCancel = new Button();
            btnCreate = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(txtName);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(327, 60);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Oda Bilgisi";
            // 
            // label1
            // 
            label1.Location = new Point(6, 22);
            label1.Name = "label1";
            label1.Size = new Size(81, 23);
            label1.TabIndex = 1;
            label1.Text = "Oda adı:";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtName.Location = new Point(127, 22);
            txtName.Name = "txtName";
            txtName.Size = new Size(194, 23);
            txtName.TabIndex = 0;
            txtName.TextChanged += txtName_TextChanged;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(txtPassword);
            groupBox2.Controls.Add(lblPassword);
            groupBox2.Controls.Add(chkEncrypt);
            groupBox2.Location = new Point(12, 78);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(327, 80);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Güvenlik Ayarları";
            // 
            // txtPassword
            // 
            txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtPassword.Enabled = false;
            txtPassword.Location = new Point(127, 48);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(194, 23);
            txtPassword.TabIndex = 2;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.TextChanged += txtPassword_TextChanged;
            // 
            // lblPassword
            // 
            lblPassword.Enabled = false;
            lblPassword.Location = new Point(6, 48);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(100, 23);
            lblPassword.TabIndex = 1;
            lblPassword.Text = "Oda Şifresi: ";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkEncrypt
            // 
            chkEncrypt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chkEncrypt.Location = new Point(6, 22);
            chkEncrypt.Name = "chkEncrypt";
            chkEncrypt.Size = new Size(315, 23);
            chkEncrypt.TabIndex = 0;
            chkEncrypt.Text = "Uçtan Uca Şifreleme";
            chkEncrypt.UseVisualStyleBackColor = true;
            chkEncrypt.CheckedChanged += chkEncrypt_CheckedChanged;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(264, 173);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "İptal Et";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnCreate
            // 
            btnCreate.Enabled = false;
            btnCreate.Location = new Point(157, 173);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(101, 23);
            btnCreate.TabIndex = 3;
            btnCreate.Text = "Oda Oluştur";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // ChatClientRoomCreateForm
            // 
            AcceptButton = btnCreate;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(351, 200);
            Controls.Add(btnCreate);
            Controls.Add(btnCancel);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "ChatClientRoomCreateForm";
            Text = "Oda Oluştur";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private Label label1;
        private TextBox txtName;
        private GroupBox groupBox2;
        private TextBox txtPassword;
        private Label lblPassword;
        private CheckBox chkEncrypt;
        private Button btnCancel;
        private Button btnCreate;
    }
}