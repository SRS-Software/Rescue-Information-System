#region

using System;

#endregion

namespace RIS.Core.Riverlevel
{
    public class DataReceivedEventArgs : EventArgs
    {
        public string Description { get; set; }
        public string Riverlevel_Description { get; set; }
        public string Riverlevel_Value { get; set; }
        public string Flowspeed_Description { get; set; }
        public string Flowspeed_Value { get; set; }
        public string Warning { get; set; }
        public string DataDate { get; set; }
    }
}