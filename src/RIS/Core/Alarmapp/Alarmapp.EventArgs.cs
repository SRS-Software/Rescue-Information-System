#region

using System;

#endregion

namespace RIS.Core.Alarmapp
{
    public class AlarmedEventArgs : EventArgs
    {
        public AlarmedEventArgs(string _einsatzGuid, string _operationId)
        {
            EinsatzGuid = _einsatzGuid;
            OperationId = _operationId;
        }

        public string EinsatzGuid { get; }
        public string OperationId { get; }
    }

    public class UpdatedEventArgs : EventArgs
    {
        public UpdatedEventArgs(string einsatzGuid, AlarmStatus status)
        {
            EinsatzGuid = einsatzGuid;
            Status = status;
        }

        public string EinsatzGuid { get; }
        public AlarmStatus Status { get; }
    }
}