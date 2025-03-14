using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMT.Copilot
{
    public class HMTChatMessage
    {
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsUser { get; set; }

        public HMTChatMessage(string _content, bool _isUser)
        {
            Content = _content;
            IsUser = _isUser;
        }
    }
}
