﻿using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ChatUygulamasi.ChatClient
{
    public partial class ReadOnlyRichTextBox : RichTextBox
    {
        [DllImport("user32.dll")]
        public static extern bool HideCaret(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool GetScrollRange(IntPtr hwnd, int nBar, out int lpMinPos, out int lpMaxPos);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, Int32 wMsg, Int32 wParam, ref Point lParam);

        public event EventHandler? ScrolledToBottom;

        public event EventHandler? UnscrolledFromBottom;

        private bool m_WasBottom = false;

        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x20A;
        private const int WM_USER = 0x400;
        private const int SB_VERT = 1;
        private const int EM_SETSCROLLPOS = WM_USER + 222;
        private const int EM_GETSCROLLPOS = WM_USER + 221;

        public ReadOnlyRichTextBox()
        {
            InitializeComponent();

            this.ReadOnly = true;
        }

        public ReadOnlyRichTextBox(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public void ScrollToBottom()
        {
            SelectionStart = Text.Length;
            ScrollToCaret();

            OnScrolledToBottom(EventArgs.Empty);
        }

        public bool IsAtMaxScroll()
        {
            int minScroll, maxScroll;
            GetScrollRange(Handle, SB_VERT, out minScroll, out maxScroll);

            Point scrollPos = Point.Empty;
            SendMessage(Handle, EM_GETSCROLLPOS, 0, ref scrollPos);

            return scrollPos.Y + ClientSize.Height >= maxScroll;
        }

        protected virtual void OnScrolledToBottom(EventArgs e)
        {
            if (ScrolledToBottom != null)
                ScrolledToBottom(this, e);

            m_WasBottom = true;
        }

        protected virtual void OnUnscrolledFromBottom(EventArgs e)
        {
            if (!m_WasBottom)
            {
                return;
            }
            m_WasBottom = false;

            if (UnscrolledFromBottom != null)
                UnscrolledFromBottom(this, e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (IsAtMaxScroll())
                OnScrolledToBottom(EventArgs.Empty);
            else
                OnUnscrolledFromBottom(EventArgs.Empty);

            base.OnKeyUp(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL)
            {
                if (IsAtMaxScroll())
                    OnScrolledToBottom(EventArgs.Empty);
                else
                    OnUnscrolledFromBottom(EventArgs.Empty);
            }

            base.WndProc(ref m);

            HideCaret(this.Handle);
        }
    }
}
