#region

using System;

#endregion

namespace RIS.Core.Ams
{
    public class EinsatzFinishedEventArgs : EventArgs
    {
        public EinsatzFinishedEventArgs()
        {
        }

        public EinsatzFinishedEventArgs(Einsatz einsatz)
        {
            Einsatz = einsatz;
        }

        public Einsatz Einsatz { get; set; }
    }
}