#region

using System;

#endregion

namespace RIS.Core.Fax
{
    public class EinsatzCreatedEventArgs : EventArgs
    {
        public EinsatzCreatedEventArgs(Einsatz _einsatz)
        {
            Einsatz = _einsatz;
        }

        public Einsatz Einsatz { get; }
    }
}