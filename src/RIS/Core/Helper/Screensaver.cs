#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

#endregion

namespace RIS.Core.Helper
{
    public class Screensaver
    {
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000
        }

        public static bool Status;

        // Signatures for unmanaged calls
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        /// <summary>
        /// </summary>
        public static void Enable()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            Status = true;
        }

        /// <summary>
        /// </summary>
        public static void Disable()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                                    EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            Status = false;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static bool IsRunning()
        {
            var _processCount = Process.GetProcesses().Where(x => x.ProcessName.EndsWith("scr")).Count();
            return _processCount > 0;
        }

        /// <summary>
        /// </summary>
        public static void Kill()
        {
            var _processList = Process.GetProcesses().Where(x => x.ProcessName.EndsWith("scr")).ToList();
            foreach (var _process in _processList) _process.Kill();
        }
    }
}