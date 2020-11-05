#region

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using SRS.Utilities;

#endregion

namespace RIS.Core.Helper
{
    public class WaitFileReady
    {
        public static bool Check(string _fileName)
        {
            if (File.Exists(_fileName) == false)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Output file {_fileName} does not exist.");
                return false;
            }

            var waitFileReadyElapsed = DateTime.Now.AddSeconds(30);
            while (DateTime.Now < waitFileReadyElapsed)
            {
                try
                {
                    using (Stream stream = File.Open(_fileName, FileMode.Open, FileAccess.Read))
                    {
                        //Check if file stream valid
                        if (stream == null)
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Output file {_fileName} stream is null");
                        }
                        //Check if file size is zero
                        else if (stream.Length == 0)
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Output file {_fileName} length is zero");
                        }
                        else
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Output file {_fileName} ready.");
                            return true;
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                        $"Output file {_fileName} not yet ready ({ex.Message})");
                }
                catch (IOException ex)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                        $"Output file {_fileName} not yet ready ({ex.Message})");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                        $"Output file {_fileName} not yet ready ({ex.Message})");
                }

                Thread.Sleep(1000);
            }

            return false;
        }
    }
}