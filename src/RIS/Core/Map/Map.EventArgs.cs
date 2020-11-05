#region

using System;
using RIS.Core.Fax;

#endregion

namespace RIS.Core.Map
{
    public class FinishedEventArgs : EventArgs
    {
        public FinishedEventArgs(Einsatz _einsatz)
        {
            Einsatz = _einsatz;
        }

        public Einsatz Einsatz { get; }

        public bool Found { get; set; }
        public double? Distance { get; set; }
        public byte[] ImageWindow { get; set; }
        public byte[] ImageReport { get; set; }
        public string Description { get; set; }
    }
}