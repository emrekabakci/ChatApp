namespace ChatUygulamasi.ChatClient
{
    partial class ChatClientForm
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
            components = new System.ComponentModel.Container();
            lblHeading = new Label();
            btnSend = new Button();
            txtCompose = new TextBox();
            splitContainer1 = new SplitContainer();
            btnScrollToBottom = new Button();
            txtMessages = new ReadOnlyRichTextBox(components);
            splitContainer2 = new SplitContainer();
            lstRooms = new ChannelListBox(components);
            label1 = new Label();
            lstChannels = new ChannelListBox(components);
            lblServer = new Label();
            btnConnect = new Button();
            lblMyName = new Label();
            label2 = new Label();
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            logOutToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            roomToolStripMenuItem = new ToolStripMenuItem();
            createRoomToolStripMenuItem = new ToolStripMenuItem();
            deleteRoomToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeading
            // 
            lblHeading.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblHeading.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            lblHeading.Location = new Point(3, 3);
            lblHeading.Name = "lblHeading";
            lblHeading.Size = new Size(417, 27);
            lblHeading.TabIndex = 0;
            lblHeading.Text = "Başlık";
            lblHeading.TextAlign = ContentAlignment.BottomLeft;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(457, 383);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 4;
            btnSend.Text = "Gönder";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtCompose
            // 
            txtCompose.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtCompose.Location = new Point(3, 383);
            txtCompose.Name = "txtCompose";
            txtCompose.Size = new Size(448, 23);
            txtCompose.TabIndex = 5;
            txtCompose.KeyDown += txtCompose_KeyDown;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Location = new Point(12, 27);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(btnScrollToBottom);
            splitContainer1.Panel1.Controls.Add(txtMessages);
            splitContainer1.Panel1.Controls.Add(txtCompose);
            splitContainer1.Panel1.Controls.Add(lblHeading);
            splitContainer1.Panel1.Controls.Add(btnSend);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(776, 411);
            splitContainer1.SplitterDistance = 537;
            splitContainer1.TabIndex = 6;
            // 
            // btnScrollToBottom
            // 
            btnScrollToBottom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnScrollToBottom.Enabled = false;
            btnScrollToBottom.Location = new Point(426, 3);
            btnScrollToBottom.Name = "btnScrollToBottom";
            btnScrollToBottom.Size = new Size(106, 24);
            btnScrollToBottom.TabIndex = 7;
            btnScrollToBottom.Text = "En aşağı in";
            btnScrollToBottom.UseVisualStyleBackColor = true;
            btnScrollToBottom.Click += btnScrollToBottom_Click;
            // 
            // txtMessages
            // 
            txtMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMessages.BackColor = SystemColors.Window;
            txtMessages.BorderStyle = BorderStyle.FixedSingle;
            txtMessages.Font = new Font("Lucida Console", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtMessages.Location = new Point(3, 33);
            txtMessages.Name = "txtMessages";
            txtMessages.ReadOnly = true;
            txtMessages.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            txtMessages.ShortcutsEnabled = false;
            txtMessages.Size = new Size(529, 337);
            txtMessages.TabIndex = 6;
            txtMessages.TabStop = false;
            txtMessages.Text = "";
            txtMessages.TextChanged += txtMessages_TextChanged;
            // 
            // splitContainer2
            // 
            splitContainer2.BorderStyle = BorderStyle.FixedSingle;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(lstRooms);
            splitContainer2.Panel1.Controls.Add(label1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(lstChannels);
            splitContainer2.Panel2.Controls.Add(lblServer);
            splitContainer2.Panel2.Controls.Add(btnConnect);
            splitContainer2.Panel2.Controls.Add(lblMyName);
            splitContainer2.Panel2.Controls.Add(label2);
            splitContainer2.Size = new Size(235, 411);
            splitContainer2.SplitterDistance = 139;
            splitContainer2.TabIndex = 0;
            // 
            // lstRooms
            // 
            lstRooms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstRooms.FormattingEnabled = true;
            lstRooms.IntegralHeight = false;
            lstRooms.ItemHeight = 15;
            lstRooms.Location = new Point(3, 33);
            lstRooms.Name = "lstRooms";
            lstRooms.Size = new Size(227, 101);
            lstRooms.TabIndex = 15;
            lstRooms.SelectedIndexChanged += lstRooms_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(227, 27);
            label1.TabIndex = 13;
            label1.Text = "Gruplar:";
            label1.TextAlign = ContentAlignment.BottomLeft;
            // 
            // lstChannels
            // 
            lstChannels.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstChannels.FormattingEnabled = true;
            lstChannels.IntegralHeight = false;
            lstChannels.ItemHeight = 15;
            lstChannels.Location = new Point(3, 23);
            lstChannels.Name = "lstChannels";
            lstChannels.Size = new Size(227, 184);
            lstChannels.TabIndex = 14;
            lstChannels.SelectedIndexChanged += lstChannels_SelectedIndexChanged;
            // 
            // lblServer
            // 
            lblServer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblServer.AutoEllipsis = true;
            lblServer.Location = new Point(3, 238);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(133, 25);
            lblServer.TabIndex = 12;
            lblServer.Text = "Server: ";
            lblServer.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnConnect
            // 
            btnConnect.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnConnect.AutoSize = true;
            btnConnect.Enabled = false;
            btnConnect.Location = new Point(142, 238);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(88, 25);
            btnConnect.TabIndex = 11;
            btnConnect.Text = "Bağlanıyor...";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // lblMyName
            // 
            lblMyName.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblMyName.Location = new Point(3, 210);
            lblMyName.Name = "lblMyName";
            lblMyName.Size = new Size(227, 25);
            lblMyName.TabIndex = 10;
            lblMyName.Text = "Hoşgeldin: ";
            lblMyName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(227, 20);
            label2.TabIndex = 9;
            label2.Text = "Kişiler:";
            label2.TextAlign = ContentAlignment.BottomLeft;
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, roomToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(800, 24);
            menuStrip.TabIndex = 7;
            menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { logOutToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(51, 20);
            fileToolStripMenuItem.Text = "Dosya";
            // 
            // logOutToolStripMenuItem
            // 
            logOutToolStripMenuItem.Name = "logOutToolStripMenuItem";
            logOutToolStripMenuItem.Size = new Size(156, 22);
            logOutToolStripMenuItem.Text = "Oturumu Kapat";
            logOutToolStripMenuItem.Click += logOutToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(156, 22);
            exitToolStripMenuItem.Text = "Çıkış Yap";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // roomToolStripMenuItem
            // 
            roomToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createRoomToolStripMenuItem, deleteRoomToolStripMenuItem });
            roomToolStripMenuItem.Name = "roomToolStripMenuItem";
            roomToolStripMenuItem.Size = new Size(45, 20);
            roomToolStripMenuItem.Text = "Grup";
            roomToolStripMenuItem.DropDownOpening += roomToolStripMenuItem_DropDownOpening;
            // 
            // createRoomToolStripMenuItem
            // 
            createRoomToolStripMenuItem.Name = "createRoomToolStripMenuItem";
            createRoomToolStripMenuItem.Size = new Size(142, 22);
            createRoomToolStripMenuItem.Text = "Grup Oluştur";
            createRoomToolStripMenuItem.Click += createRoomToolStripMenuItem_Click;
            // 
            // deleteRoomToolStripMenuItem
            // 
            deleteRoomToolStripMenuItem.Enabled = false;
            deleteRoomToolStripMenuItem.Name = "deleteRoomToolStripMenuItem";
            deleteRoomToolStripMenuItem.Size = new Size(142, 22);
            deleteRoomToolStripMenuItem.Text = "Grup Sil";
            // 
            // ChatClientForm
            // 
            AcceptButton = btnSend;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "ChatClientForm";
            Text = "Chat";
            FormClosing += ChatClientForm_FormClosing;
            Resize += ChatClientForm_Resize;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblHeading;
        private Button btnSend;
        private TextBox txtCompose;
        private SplitContainer splitContainer1;
        private ReadOnlyRichTextBox txtMessages;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem logOutToolStripMenuItem;
        private Button btnScrollToBottom;
        private ToolStripMenuItem roomToolStripMenuItem;
        private ToolStripMenuItem createRoomToolStripMenuItem;
        private ToolStripMenuItem deleteRoomToolStripMenuItem;
        private SplitContainer splitContainer2;
        private ChannelListBox lstRooms;
        private Label label1;
        private ChannelListBox lstChannels;
        private Label lblServer;
        private Button btnConnect;
        private Label lblMyName;
        private Label label2;
    }
}