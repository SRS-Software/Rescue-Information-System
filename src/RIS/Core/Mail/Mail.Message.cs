#region

using System.Collections.Generic;
using RIS.Model;

#endregion

namespace RIS.Core.Mail
{
    public class Message
    {
        public Message()
        {
            Recivers = new List<User>();
        }

        public Message(List<User> _reciever)
        {
            Recivers = _reciever;
            if (Recivers == null) Recivers = new List<User>();
        }

        public List<User> Recivers { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string AttachmentPath { get; set; }
    }
}