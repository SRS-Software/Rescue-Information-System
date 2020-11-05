#region

using System;
using RIS.Model;

#endregion

namespace RIS.Core.Decoder
{
    public class StatusEventArgs : EventArgs
    {
        public int Number { get; set; }
        public string Status { get; set; }
    }

    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs()
        {
            Time = DateTime.Now;
        }

        public MessageEventArgs(string data) : this()
        {
            Data = data;
        }


        public DateTime Time { get; }
        public string Data { get; }
    }

    public class FmsMessageEventArgs : EventArgs
    {
        public FmsMessageEventArgs()
        {
            Time = DateTime.Now;
        }

        public FmsMessageEventArgs(string _identifier, string _status) : this()
        {
            Identifier = _identifier;
            Status = _status;
        }


        public DateTime Time { get; }
        public string Identifier { get; }
        public string Status { get; }
        public Vehicle Vehicle { get; set; }
    }

    public class PagerMessageEventArgs : EventArgs
    {
        public PagerMessageEventArgs()
        {
            Time = DateTime.Now;
        }

        public PagerMessageEventArgs(string identifier, string message) : this()
        {
            Identifier = identifier;
            Message = message;
        }


        public DateTime Time { get; }
        public string Identifier { get; }
        public string Message { get; }
        public Pager Pager { get; set; }
    }
}