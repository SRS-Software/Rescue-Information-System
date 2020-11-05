#region

using System.IO;

#endregion

namespace RIS.Core.Helper
{
    public class MoveFile
    {
        /// <summary>
        ///     Verschieben der Faxdatei
        /// </summary>
        public static string Start(string sourceFile, string destinationFile)
        {
            //Check if input okey and exist
            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(destinationFile) ||
                !WaitFileReady.Check(sourceFile))
                return null;

            //Check if new file already exist and delete
            if (File.Exists(destinationFile))
            {
                if (WaitFileReady.Check(destinationFile))
                    File.Delete(destinationFile);
                else
                    return null;
            }

            //Move and wait until ok
            File.Move(sourceFile, destinationFile);
            if (!WaitFileReady.Check(destinationFile)) return null;

            return destinationFile;
        }
    }
}