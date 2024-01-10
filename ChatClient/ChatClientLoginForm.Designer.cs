namespace ChatUygulamasi.ChatClient
{
    partial class ChatClientLoginForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            txtHostname = new TextBox();
            label2 = new Label();
            txtNickname = new TextBox();
            btnLogin = new Button();
            cboxLan = new ComboBox();
            radUseHostname = new RadioButton();
            radUseLan = new RadioButton();
            label3 = new Label();
            button1 = new Button();
            textBox1 = new TextBox();
            label4 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(244, 15);
            label1.TabIndex = 0;
            label1.Text = "Chat Sunucusunun IP adresini giriniz:";
            label1.TextAlign = ContentAlignment.TopCenter;
            // 
            // txtHostname
            // 
            txtHostname.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtHostname.Location = new Point(34, 27);
            txtHostname.Name = "txtHostname";
            txtHostname.Size = new Size(222, 23);
            txtHostname.TabIndex = 1;
            txtHostname.Text = "localhost";
            txtHostname.MouseClick += txtHostname_MouseClick;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label2.Location = new Point(12, 105);
            label2.Name = "label2";
            label2.Size = new Size(244, 23);
            label2.TabIndex = 2;
            label2.Text = "KullanıcıAdı Giriniz:";
            label2.TextAlign = ContentAlignment.BottomCenter;
            label2.Click += label2_Click;
            // 
            // txtNickname
            // 
            txtNickname.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNickname.Location = new Point(12, 131);
            txtNickname.Name = "txtNickname";
            txtNickname.Size = new Size(244, 23);
            txtNickname.TabIndex = 3;
            txtNickname.Text = "KullanıcıAdı";
            // 
            // btnLogin
            // 
            btnLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLogin.Location = new Point(12, 212);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(244, 23);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "&Giriş Yap";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // cboxLan
            // 
            cboxLan.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboxLan.DropDownStyle = ComboBoxStyle.DropDownList;
            cboxLan.Enabled = false;
            cboxLan.FormattingEnabled = true;
            cboxLan.Location = new Point(34, 79);
            cboxLan.Name = "cboxLan";
            cboxLan.Size = new Size(222, 23);
            cboxLan.TabIndex = 5;
            cboxLan.MouseClick += cboxLan_MouseClick;
            // 
            // radUseHostname
            // 
            radUseHostname.CheckAlign = ContentAlignment.MiddleCenter;
            radUseHostname.Checked = true;
            radUseHostname.Location = new Point(12, 27);
            radUseHostname.Name = "radUseHostname";
            radUseHostname.Size = new Size(16, 23);
            radUseHostname.TabIndex = 6;
            radUseHostname.TabStop = true;
            radUseHostname.UseVisualStyleBackColor = true;
            // 
            // radUseLan
            // 
            radUseLan.CheckAlign = ContentAlignment.MiddleCenter;
            radUseLan.Enabled = false;
            radUseLan.Location = new Point(12, 79);
            radUseLan.Name = "radUseLan";
            radUseLan.Size = new Size(16, 23);
            radUseLan.TabIndex = 7;
            radUseLan.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label3.Location = new Point(12, 53);
            label3.Name = "label3";
            label3.Size = new Size(244, 23);
            label3.TabIndex = 8;
            label3.Text = "Ya da LAN üzerinden bir ağ seçiniz:";
            label3.TextAlign = ContentAlignment.BottomCenter;
            // 
            // button1
            // 
            button1.Location = new Point(12, 241);
            button1.Name = "button1";
            button1.Size = new Size(244, 23);
            button1.TabIndex = 9;
            button1.Text = "Kayıt Ol";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(12, 183);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(244, 23);
            textBox1.TabIndex = 10;
            textBox1.Text = "Şifre";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label4.Location = new Point(12, 157);
            label4.Name = "label4";
            label4.Size = new Size(244, 23);
            label4.TabIndex = 11;
            label4.Text = "Şifre Giriniz";
            label4.TextAlign = ContentAlignment.BottomCenter;
            // 
            // ChatClientLoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(268, 288);
            Controls.Add(label4);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(radUseLan);
            Controls.Add(radUseHostname);
            Controls.Add(cboxLan);
            Controls.Add(btnLogin);
            Controls.Add(txtNickname);
            Controls.Add(label2);
            Controls.Add(txtHostname);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ChatClientLoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chat Uygulaması";
            FormClosing += ChatClientLoginForm_FormClosing;
            Load += Form1_Load;
            VisibleChanged += ChatClientLoginForm_VisibleChanged;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtHostname;
        private Label label2;
        private TextBox txtNickname;
        private Button btnLogin;
        private ComboBox cboxLan;
        private RadioButton radUseHostname;
        private RadioButton radUseLan;
        private Label label3;
        private Button button1;
        private TextBox textBox1;
        private Label label4;
    }
}