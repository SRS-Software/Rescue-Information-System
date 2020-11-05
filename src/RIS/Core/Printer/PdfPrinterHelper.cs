#region

using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using SRS.Utilities;

#endregion

namespace RIS.Core.Printer
{
    public class PdfPrinterHelper
    {
        public static bool Print(string _printerName, string _filePath)
        {
            var _adobePath = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft")
                .OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("App Paths").OpenSubKey("AcroRd32.exe")
                .GetValue("");
            if (string.IsNullOrEmpty(_adobePath.ToString()))
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "PdfPrinterHelper: Adobe PDF-Reader not found or wrong version");
                return false;
            }

            var psInfo = new ProcessStartInfo();
            psInfo.FileName = _adobePath.ToString();
            psInfo.Arguments = $"/t /h \"{_filePath}\" \"{_printerName}\"";
            psInfo.WindowStyle = ProcessWindowStyle.Hidden;
            psInfo.CreateNoWindow = true;

            var process = Process.Start(psInfo);
            return process.WaitForExit(5000);
        }

        public static bool Print(string _printerName, string _filePath, int _copies)
        {
            var _adobePath = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft")
                .OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("App Paths").OpenSubKey("AcroRd32.exe")
                .GetValue("");
            if (string.IsNullOrEmpty(_adobePath.ToString()))
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "PdfPrinterHelper: Adobe PDF-Reader not found or wrong version");
                return false;
            }

            var psInfo = new ProcessStartInfo();
            psInfo.FileName = _adobePath.ToString();
            psInfo.Arguments = $"/t /h \"{_filePath}\" \"{_printerName}\"";
            psInfo.WindowStyle = ProcessWindowStyle.Hidden;
            psInfo.CreateNoWindow = true;

            for (var copy = 1; copy <= _copies; copy++) Process.Start(psInfo);

            return true;
        }
    }
}