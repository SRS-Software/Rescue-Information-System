#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace RIS.Core.Helper
{
    public class Screen
    {
        // Constants
        private const int HWND_BROADCAST = 0x10014;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MONITORPOWER = 0xF170;
        private const int MONITOR_ON = -1;
        private const int MONITOR_OFF = 2;
        private const int MONITOR_STANDBY = 1;
        private const int MOUSEEVENTF_MOVE = 0x0001;


        public static bool Status;

        // Signatures for unmanaged calls
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr handle, int msg, int wparam, int lparam);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

        /// <summary>
        /// </summary>
        public static void SwitchOn()
        {
            SendMessage((IntPtr) HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_ON);
            mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero);
            Status = true;
        }

        /// <summary>
        /// </summary>
        public static void SwitchOff()
        {
            SendMessage((IntPtr) HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF);
            Status = false;
        }
    }
}