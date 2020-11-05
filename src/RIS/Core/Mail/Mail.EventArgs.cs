#region

using System;

#endregion

namespace RIS.Core.Mail
{
    public class MailReceivedEventArgs : EventArgs
    {
        public MailReceivedEventArgs(string _message)
        {
            Message = _message;
        }

        public string Message { get; }
    }
}