using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatUygulamasi.ChatClient
{
    public class ChatRecipient
    {
        public string nickname;

        public bool isJoined;

        public ChatRecipient(string nickname, bool joined)
        {
            this.nickname = nickname;
            this.isJoined = joined;
        }
    }
}
