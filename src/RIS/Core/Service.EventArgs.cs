#region

using System;
using System.Reflection;

#endregion

namespace RIS.Core
{
    public class ExceptionEventArgs : EventArgs
    {
        public MethodBase Methode { get; set; }
        public Exception Error { get; set; }
        public string Message { get; set; }
    }
}