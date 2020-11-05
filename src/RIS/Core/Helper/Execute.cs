#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;

#endregion

namespace RIS.Core.Helper
{
    public static class Execute
    {
        public static void SoundOrApp(string _path)
        {
            if (string.IsNullOrWhiteSpace(_path)) return;

            string[] mediaExtensions =
            {
                ".WAV",
                ".MID",
                ".MIDI",
                ".WMA",
                ".MP3",
                ".OGG"
            };

            //Split arguments parameters
            var _applicationString = _path.Split('/');
            if (!_applicationString.Any() || !File.Exists(_applicationString[0])) return;

            if (mediaExtensions.Contains(Path.GetExtension(_applicationString[0]), StringComparer.OrdinalIgnoreCase))
            {
                using (var _waveOut = new WaveOutEvent())
                {
                    using (var _audioFileReader = new AudioFileReader(_applicationString[0]))
                    {
                        _waveOut.Init(_audioFileReader);
                        _waveOut.Play();

                        while (_waveOut.PlaybackState == PlaybackState.Playing) Task.Delay(500);
                    }
                }
            }
            else
            {
                //Split arguments
                var _applicationArguments = "";
                for (var i = 1; i < _applicationString.Count(); i++)
                    _applicationArguments += _applicationString[i] + " ";

                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = _applicationString[0],
                    Arguments = _applicationArguments
                };
                process.Start();
            }
        }
    }
}