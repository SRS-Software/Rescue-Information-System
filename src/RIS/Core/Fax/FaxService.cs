#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Puma.Net;
using RIS.Business;
using RIS.Core.Fax;
using RIS.Core.Helper;
using RIS.Model;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;
using Timer = System.Timers.Timer;

#endregion

namespace RIS.Core
{
    public class FaxService : IFaxService
    {
        private readonly IBusiness _business;
        private readonly object _ocrLocker = new object();
        private readonly FileSystemWatcher _pdfFileSystemWatcher;
        private readonly FileSystemWatcher _sffFileSystemWatcher;
        private readonly FileSystemWatcher _tifFileSystemWatcher;
        private readonly FileSystemWatcher _txtFileSystemWatcher;

        public FaxService(IBusiness business)
        {
            try
            {
                _business = business;

                //Create fileWatcher Instances
                _pdfFileSystemWatcher = new FileSystemWatcher
                {
                    Filter = "*.pdf"
                };
                _pdfFileSystemWatcher.Error += FileSystemWatcher_Error;
                _pdfFileSystemWatcher.Created += (sender, e) =>
                {
                    Task.Factory.StartNew(() => PdfFileSystemWatcher_Created(sender, e));
                };

                _txtFileSystemWatcher = new FileSystemWatcher
                {
                    Filter = "*.txt"
                };
                _txtFileSystemWatcher.Error += FileSystemWatcher_Error;
                _txtFileSystemWatcher.Created += (sender, e) =>
                {
                    Task.Factory.StartNew(() => TxtFileSystemWatcher_Created(sender, e));
                };

                _tifFileSystemWatcher = new FileSystemWatcher
                {
                    Filter = "*.tif"
                };
                _tifFileSystemWatcher.Error += FileSystemWatcher_Error;
                _tifFileSystemWatcher.Created += (sender, e) =>
                {
                    Task.Factory.StartNew(() => TifFileSystemWatcher_Created(sender, e));
                };

                _sffFileSystemWatcher = new FileSystemWatcher
                {
                    Filter = "*.sff"
                };
                _sffFileSystemWatcher.Error += FileSystemWatcher_Error;
                _sffFileSystemWatcher.Created += (sender, e) =>
                {
                    Task.Factory.StartNew(() => SffFileSystemWatcher_Created(sender, e));
                };

                _folderReconnectTimer = new Timer();
                _folderReconnectTimer.AutoReset = false;
                //_folderReconnectTimer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
                _folderReconnectTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
                _folderReconnectTimer.Elapsed += FolderReconnectTimer_Elapsed;

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Initialize");
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Properties

        public bool IsRunning { get; private set; }

        #endregion //Public Properties

        #region Public Funtions

        public void Start()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                IsRunning = false;

                //Pfade überprüfen
                if (!Directory.Exists(Settings.Default.Fax_PathInput))
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), "Ordner Faxeingang nicht gefunden");

                    // if exception occur try reestablish every minute
                    if (_folderReconnectTimer.Enabled == false)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "_folderReconnectTimer -> start");
                        _folderReconnectTimer.Start();
                    }

                    return;
                }

                if (!Directory.Exists(Settings.Default.Fax_PathArchive))
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), "Ordner Faxarchiv nicht gefunden");
                    return;
                }

                //Load database lists
                _filterList = _business.GetAllFilter().ToList();
                _aaoList = _business.GetAllAao().ToList();
                _einsatzmittelList = _business.GetVehiclesAreEinsatzmittel().ToList();

                //Start all FileWatcher
                _tifFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _tifFileSystemWatcher.EnableRaisingEvents = true;
                _sffFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _sffFileSystemWatcher.EnableRaisingEvents = true;
                _pdfFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _pdfFileSystemWatcher.EnableRaisingEvents = true;
                _txtFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _txtFileSystemWatcher.EnableRaisingEvents = true;

                IsRunning = true;

                stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Started -> {stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        public void Stop()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Stopping");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                _folderReconnectTimer.Stop();
                _faxLockedElapsed = DateTime.Now;

                if (_tifFileSystemWatcher != null) _tifFileSystemWatcher.EnableRaisingEvents = false;

                if (_sffFileSystemWatcher != null) _sffFileSystemWatcher.EnableRaisingEvents = false;

                if (_pdfFileSystemWatcher != null) _pdfFileSystemWatcher.EnableRaisingEvents = false;

                if (_txtFileSystemWatcher != null) _txtFileSystemWatcher.EnableRaisingEvents = false;


                stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Stopped -> {stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
            finally
            {
                IsRunning = false;
            }
        }

        public string TestOcr(string path)
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "TestOcr");

                //Wait for access      
                if (!WaitFileReady.Check(path))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Open file failed -> abort");
                    return null;
                }

                //Execute ocr and get result as string    
                string faxText;
                lock (_ocrLocker)
                {
                    var ocrPage = new PumaPage
                    {
                        Language = PumaLanguage.German,
                        AutoRotateImage = true,
                        ImproveFax100 = true,
                        UseTextFormating = true,
                        RecognizePictures = true
                    };
                    ocrPage.LoadImage(path);
                    faxText = ocrPage.RecognizeToString();
                    ocrPage.Dispose();
                }

                return faxText;
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
                return null;
            }
        }

        #endregion //Public Funtions

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;
        public event EventHandler<EinsatzCreatedEventArgs> EinsatzCreated;

        #endregion //Events

        #region Private Properties

        private DateTime? _faxLockedElapsed;
        private readonly Timer _folderReconnectTimer;

        private List<Filter> _filterList;
        private List<Aao> _aaoList;
        private List<Vehicle> _einsatzmittelList;

        #endregion //Private Properties

        #region Private Funtions

        /// <summary>
        ///     FileSystemWatcher ErrorEventHandler
        /// </summary>
        private void FileSystemWatcher_Error(object source, ErrorEventArgs e)
        {
            Logger.WriteError(MethodBase.GetCurrentMethod(), "FileSystemWatcher_Error -> " + e.GetException()?.Message);
            _tifFileSystemWatcher.EnableRaisingEvents = false;
            _sffFileSystemWatcher.EnableRaisingEvents = false;
            _pdfFileSystemWatcher.EnableRaisingEvents = false;
            _txtFileSystemWatcher.EnableRaisingEvents = false;

            // if exception occur try reestablish every minute
            if (_folderReconnectTimer.Enabled == false)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "_folderReconnectTimer -> start");
                _folderReconnectTimer.Start();
            }
        }

        private void FolderReconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "_folderReconnectTimer -> elapsed");

            try
            {
                if (!Directory.Exists(Settings.Default.Fax_PathInput))
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), "Ordner Faxeingang nicht gefunden");
                    _folderReconnectTimer.Start();
                    return;
                }

                //Start all FileWatcher
                _tifFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _tifFileSystemWatcher.EnableRaisingEvents = true;
                _sffFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _sffFileSystemWatcher.EnableRaisingEvents = true;
                _pdfFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _pdfFileSystemWatcher.EnableRaisingEvents = true;
                _txtFileSystemWatcher.Path = Settings.Default.Fax_PathInput;
                _txtFileSystemWatcher.EnableRaisingEvents = true;
            }
            catch
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "_folderReconnectTimer -> start");
                _folderReconnectTimer.Start();
            }
        }


        /// <summary>
        ///     EventHandler wird ausgelöst wenn eine neue *.sff Datei erstellt wurde
        ///     Wandelt sff in tif um
        /// </summary>
        private void SffFileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Performance test
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                //Wait to create file
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "SFF -> received");

                //Auf Zugriff warten -> false return
                var path = e.FullPath;
                if (!WaitFileReady.Check(path))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "open file failed -> abort");
                    return;
                }

                //Start convert with batch
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "SFF -> start to convert");
                var psInfo = new ProcessStartInfo
                {
                    FileName = $"{Application.StartupPath}\\Tools\\SFF2TIF\\SFF2TIF.exe",
                    Arguments =
                        $"\"{path}\" \"{Settings.Default.Fax_PathInput + "\\CONVERTED_SFF_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".tif"}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                var process = new Process
                {
                    StartInfo = psInfo
                };
                process.Start();
                process.WaitForExit(60000);

                //Move into archiv
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "SFF -> move to archiv");
                MoveFile.Start(path,
                    $"{Settings.Default.Fax_PathArchive}\\ALARMFAX_SFF_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}{Path.GetExtension(path)}");

                stopwatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "runtime -> " + stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        /// <summary>
        ///     EventHandler wird ausgelöst wenn eine neue *.pdf Datei erstellt wurde
        ///     Wandelt pdf in tif um
        /// </summary>
        private void PdfFileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Performance test
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                //Wait for changed event
                _pdfFileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed, 5000);

                //Auf Zugriff warten -> false return
                var path = e.FullPath;
                if (!WaitFileReady.Check(path))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "open file failed -> abort");
                    return;
                }

                //Start convert with batch
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "PDF -> start to convert");
                var psInfo = new ProcessStartInfo
                {
                    FileName = $"{Application.StartupPath}\\Tools\\PDF2TIF\\PdfConverter.exe",
                    Arguments = path,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                var process = new Process
                {
                    StartInfo = psInfo
                };
                process.Start();
                process.WaitForExit(60000);

                //Move into archiv
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "PDF -> move to archiv");
                MoveFile.Start(path,
                    $"{Settings.Default.Fax_PathArchive}\\ALARMFAX_PDF_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}{Path.GetExtension(path)}");

                stopwatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "runtime -> " + stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        /// <summary>
        ///     EventHandler wird ausgelöst wenn eine neue *.txt Datei erstellt wurde
        ///     Startet AlarmfaxThread
        /// </summary>
        private void TxtFileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Performance test
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "TXT -> received");

                //Überprüfen ob Faxempfang zulässig
                if (_faxLockedElapsed != null && DateTime.Now < _faxLockedElapsed)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Fax locked -> abort");
                    return;
                }

                _faxLockedElapsed = DateTime.Now.Add(Settings.Default.Fax_LockTime);

                //Wait for access     
                var path = e.FullPath;
                if (!WaitFileReady.Check(path))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "open file failed -> abort");
                    return;
                }

                //Read text from file    
                var faxText = string.Empty;
                using (var streamReader = new StreamReader(path, Encoding.UTF8))
                {
                    faxText = streamReader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(faxText))
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "read file failed -> abort");
                        return;
                    }
                }

                //Analyse fax
                var einsatz = StartAnalyse(faxText);
                if (einsatz == null)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "analyse failed -> abort");
                    return;
                }

                //Move fax to archive
                einsatz.FaxPath = MoveFile.Start(path,
                    $"{Settings.Default.Fax_PathArchive}\\ALARMMAIL_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}{Path.GetExtension(path)}");

                //Raise EinsatzCreated
                EinsatzCreated.RaiseEvent(this, new EinsatzCreatedEventArgs(einsatz));

                stopwatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "runtime -> " + stopwatch.Elapsed.TotalSeconds);

                //Execute File 

                Task.Factory.StartNew(() =>
                {
                    foreach (var vehicle in einsatz.Einsatzmittel)
                        //Execute App or Sound   
                        if (!string.IsNullOrWhiteSpace(vehicle.File))
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Execute -> {vehicle.File}");
                            Execute.SoundOrApp(vehicle.File);
                        }
                });
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        /// <summary>
        ///     EventHandler wird ausgelöst wenn eine neue *.tif Datei erstellt wurde
        ///     Startet AlarmfaxThread
        /// </summary>
        private void TifFileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Performance test
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "TIF received");

                //Überprüfen ob Faxempfang zulässig
                if (_faxLockedElapsed != null && DateTime.Now < _faxLockedElapsed)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Fax locked -> abort");
                    return;
                }

                _faxLockedElapsed = DateTime.Now.Add(Settings.Default.Fax_LockTime);

                //Wait for access      
                var path = e.FullPath;
                if (!WaitFileReady.Check(path))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Open file failed -> abort");
                    return;
                }

                //Execute ocr and get result as string   
                var faxText = string.Empty;
                lock (_ocrLocker)
                {
                    var ocrPage = new PumaPage
                    {
                        Language = PumaLanguage.German,
                        AutoRotateImage = true,
                        ImproveFax100 = true,
                        UseTextFormating = true,
                        RecognizePictures = true
                    };

                    var bitmapList = TiffConverter.TiffToBitmapList(path);
                    if (bitmapList == null || bitmapList.Count == 0)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Convert tif to bmp failed -> abort");
                        return;
                    }

                    foreach (var bitmap in bitmapList)
                    {
                        ocrPage.Bitmap = bitmap;
                        faxText += ocrPage.RecognizeToString();
                    }

                    //ocrPage.LoadImage(_path);
                    ocrPage.Dispose();
                }

                //Analyse fax
                var einsatz = StartAnalyse(faxText);
                if (einsatz == null)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Analyse failed -> abort");
                    return;
                }

                //Move fax to archive if analyse ok and absender valid 
                einsatz.FaxPath = MoveFile.Start(path,
                    $"{Settings.Default.Fax_PathArchive}\\ALARMFAX_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}{Path.GetExtension(path)}");

                //Raise EinsatzCreated
                EinsatzCreated.RaiseEvent(this, new EinsatzCreatedEventArgs(einsatz));

                stopwatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "runtime -> " + stopwatch.Elapsed.TotalSeconds);

                //Execute App or Sound  
                if (Settings.Default.AppExecute_OnStatus == false)
                    Task.Factory.StartNew(() =>
                    {
                        foreach (var vehicle in einsatz.Einsatzmittel)
                            //Execute App or Sound   
                            if (!string.IsNullOrWhiteSpace(vehicle.File))
                            {
                                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Execute -> {vehicle.File}");
                                Execute.SoundOrApp(vehicle.File);
                            }
                    });
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        private Einsatz StartAnalyse(string faxText)
        {
            //Check if text is valid
            if (string.IsNullOrWhiteSpace(faxText)) return null;

            //Remove Tabs
            faxText = faxText.Replace('\t', ' ');

            //Replace newline chars
            faxText = Regex.Replace(faxText, @"\r\n|\r|\n", Environment.NewLine);

            //Remove multiple whitespaces
            faxText = string.Join(" ", faxText.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

            //Load analyse settings         
            var settings = Serializer.Deserialize<SettingsOcr>(Settings.Default.Fax_OcrSettings);
            if (settings == null) return null;

            //Create new einsatz
            var result = new Einsatz();

            #region Filter "vor Auswertung"

            var einsatzmittelText = faxText;
            var bemerkungText = faxText;
            var ortText = faxText;
            var straßeText = faxText;
            var hausnummerText = faxText;
            var koordinatenRwText = faxText;
            var koordinatenHwText = faxText;
            var objektText = faxText;
            var abschnittText = faxText;
            var kreuzungText = faxText;
            var stationText = faxText;
            var schlagwortText = faxText;
            var stichwort1Text = faxText;
            var stichwort2Text = faxText;
            var stichwort3Text = faxText;

            foreach (var filter in _filterList.Where(f => f.DoBeforeShow == false))
            {
                if (string.IsNullOrWhiteSpace(filter.SearchExpression)) continue;

                switch (filter.Field.Id)
                {
                    case 1:
                        einsatzmittelText = einsatzmittelText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        bemerkungText = bemerkungText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        ortText = ortText.Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ')
                            .TrimEnd(' ');
                        straßeText = straßeText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        hausnummerText = hausnummerText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        objektText = objektText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        abschnittText = abschnittText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        kreuzungText = kreuzungText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        stationText = stationText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        schlagwortText = schlagwortText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        stichwort1Text = stichwort1Text.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        stichwort2Text = stichwort2Text.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        stichwort3Text = stichwort3Text.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 2:
                        einsatzmittelText = einsatzmittelText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 3:
                        bemerkungText = bemerkungText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 4:
                        ortText = ortText.Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ')
                            .TrimEnd(' ');
                        break;
                    case 5:
                        straßeText = straßeText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 6:
                        hausnummerText = hausnummerText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 7:
                        objektText = objektText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 8:
                        abschnittText = abschnittText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 9:
                        kreuzungText = kreuzungText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 10:
                        stationText = stationText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 11:
                        schlagwortText = schlagwortText.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 12:
                        stichwort1Text = stichwort1Text.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 13:
                        stichwort2Text = stichwort2Text.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                    case 14:
                        stichwort3Text = stichwort3Text.Replace(filter.SearchExpression, filter.ReplaceExpression)
                            .TrimStart(' ').TrimEnd(' ');
                        break;
                }
            }

            //Insert filter controls signs
            einsatzmittelText = einsatzmittelText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            bemerkungText = bemerkungText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            ortText = ortText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            straßeText = straßeText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            hausnummerText = hausnummerText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            koordinatenRwText = koordinatenRwText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            koordinatenHwText = koordinatenHwText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            objektText = objektText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            abschnittText = abschnittText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            kreuzungText = kreuzungText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            stationText = stationText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            schlagwortText = schlagwortText.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            stichwort1Text = stichwort1Text.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            stichwort2Text = stichwort2Text.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");
            stichwort3Text = stichwort3Text.Replace("%NEWLINE%", "\r\n").Replace("%TAB%", "\t");

            #endregion

            #region Analyse text

            #region Absender

            if (string.IsNullOrEmpty(settings.Absender) || Text.CheckString(faxText, settings.Absender))
                result.AbsenderValid = true;

            #endregion

            #region Einsatzmittel

            var einsatzmittelFax = new List<Vehicle>();
            foreach (var vehicle in _einsatzmittelList)
                if (Text.CheckString(einsatzmittelText, vehicle.FaxText) && !einsatzmittelFax.Contains(vehicle))
                    einsatzmittelFax.Add(vehicle);

            #endregion

            #region Bemerkung

            if (!string.IsNullOrEmpty(settings.Bemerkung.Start))
            {
                var rowNumberStart = Text.FindString(settings.Bemerkung.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Bemerkung.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Bemerkung = Text.FindLine(bemerkungText, rowNumberStart,
                            rowNumberStop == "ENDE" ? "-1" : rowNumberStop);
                    }
                    else
                    {
                        result.Bemerkung = Text.FindLine(bemerkungText, rowNumberStart);
                        result.Bemerkung = Text.FindString(result.Bemerkung, null, settings.Bemerkung.Stop);
                    }
                }
                else
                {
                    result.Bemerkung = Text.FindString(bemerkungText,
                        (settings.Bemerkung.Start_FirstWord ? Environment.NewLine : "") + settings.Bemerkung.Start,
                        settings.Bemerkung.Stop);
                    //bemerkung is valid over more lines
                }

                //Filter
                if (result.Bemerkung != null)
                {
                    result.Bemerkung = result.Bemerkung.Replace(Environment.NewLine, " "); //Remove new line
                    result.Bemerkung = string.Join(" ",
                        result.Bemerkung.Split(new[] {' '},
                            StringSplitOptions.RemoveEmptyEntries)); //Remove multiple whitespaces 
                }
            }

            #endregion

            #region Ort

            if (!string.IsNullOrEmpty(settings.Ort.Start))
            {
                var rowNumberStart = Text.FindString(settings.Ort.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Ort.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Ort = Text.FindLine(ortText, rowNumberStart,
                            Text.FindString(settings.Ort.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Ort = Text.FindLine(ortText, rowNumberStart);
                        result.Ort = Text.FindString(result.Ort, null, settings.Ort.Stop);
                    }
                }
                else
                {
                    result.Ort = Text.FindString(ortText,
                        (settings.Ort.Start_FirstWord ? Environment.NewLine : "") + settings.Ort.Start,
                        settings.Ort.Stop);
                    result.Ort = Text.FindString(result.Ort, null, Environment.NewLine);
                }
            }

            #endregion

            #region Straße

            if (!string.IsNullOrEmpty(settings.Straße.Start))
            {
                var rowNumberStart = Text.FindString(settings.Straße.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Straße.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Straße = Text.FindLine(straßeText, rowNumberStart,
                            Text.FindString(settings.Straße.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Straße = Text.FindLine(straßeText, rowNumberStart);
                        result.Straße = Text.FindString(result.Straße, null, settings.Straße.Stop);
                    }
                }
                else
                {
                    result.Straße = Text.FindString(straßeText,
                        (settings.Straße.Start_FirstWord ? Environment.NewLine : "") + settings.Straße.Start,
                        settings.Straße.Stop);
                    result.Straße = Text.FindString(result.Straße, null, Environment.NewLine);
                }
            }

            #endregion

            #region Hausnummer

            if (!string.IsNullOrEmpty(settings.Hausnummer.Start))
            {
                var rowNumberStart = Text.FindString(settings.Hausnummer.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Hausnummer.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Hausnummer = Text.FindLine(hausnummerText, rowNumberStart,
                            Text.FindString(settings.Hausnummer.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Hausnummer = Text.FindLine(hausnummerText, rowNumberStart);
                        result.Hausnummer = Text.FindString(result.Hausnummer, null, settings.Hausnummer.Stop);
                    }
                }
                else
                {
                    result.Hausnummer = Text.FindString(hausnummerText,
                        (settings.Hausnummer.Start_FirstWord ? Environment.NewLine : "") + settings.Hausnummer.Start,
                        settings.Hausnummer.Stop);
                    result.Hausnummer = Text.FindString(result.Hausnummer, null, Environment.NewLine);
                }

                //Filter
                if (result.Hausnummer != null)
                {
                    result.Hausnummer = Regex.Match(result.Hausnummer, @"[0-9]+\s*[a-z]?").Value;
                    result.Hausnummer = result.Hausnummer.Replace(" ", "");
                }
            }

            #endregion

            #region Koordinaten

            if (!string.IsNullOrEmpty(settings.KoordinatenRW.Start))
            {
                var rowNumberStart = Text.FindString(settings.KoordinatenRW.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.KoordinatenRW.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.KoordinatenRW = Text.FindLine(koordinatenRwText, rowNumberStart,
                            Text.FindString(settings.KoordinatenRW.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.KoordinatenRW = Text.FindLine(koordinatenRwText, rowNumberStart);
                        result.KoordinatenRW = Text.FindString(result.KoordinatenRW, null, settings.KoordinatenRW.Stop);
                    }
                }
                else
                {
                    result.KoordinatenRW = Text.FindString(koordinatenRwText,
                        (settings.KoordinatenRW.Start_FirstWord ? Environment.NewLine : "") +
                        settings.KoordinatenRW.Start, settings.KoordinatenRW.Stop);
                    result.KoordinatenRW = Text.FindString(result.KoordinatenRW, null, Environment.NewLine);
                }

                //Filter
                if (result.KoordinatenRW != null)
                {
                    result.KoordinatenRW = Regex.Match(result.KoordinatenRW, @"\d+").Value;
                    if (result.KoordinatenRW.Length != 7) result.KoordinatenRW = null;
                }
            }

            if (!string.IsNullOrEmpty(settings.KoordinatenHW.Start))
            {
                var rowNumberStart = Text.FindString(settings.KoordinatenHW.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.KoordinatenHW.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.KoordinatenHW = Text.FindLine(koordinatenHwText, rowNumberStart,
                            Text.FindString(settings.KoordinatenHW.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.KoordinatenHW = Text.FindLine(koordinatenHwText, rowNumberStart);
                        result.KoordinatenHW = Text.FindString(result.KoordinatenHW, null, settings.KoordinatenHW.Stop);
                    }
                }
                else
                {
                    result.KoordinatenHW = Text.FindString(koordinatenHwText,
                        (settings.KoordinatenHW.Start_FirstWord ? Environment.NewLine : "") +
                        settings.KoordinatenHW.Start, settings.KoordinatenHW.Stop);
                    result.KoordinatenHW = Text.FindString(result.KoordinatenHW, null, Environment.NewLine);
                }

                //Filter
                if (result.KoordinatenHW != null)
                {
                    result.KoordinatenHW = Regex.Match(result.KoordinatenHW, @"\d+").Value;
                    if (result.KoordinatenHW.Length != 7) result.KoordinatenHW = null;
                }
            }

            #endregion

            #region Objekt

            if (!string.IsNullOrEmpty(settings.Objekt.Start))
            {
                var rowNumberStart = Text.FindString(settings.Objekt.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Objekt.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Objekt = Text.FindLine(objektText, rowNumberStart,
                            Text.FindString(settings.Objekt.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Objekt = Text.FindLine(objektText, rowNumberStart);
                        result.Objekt = Text.FindString(result.Objekt, null, settings.Objekt.Stop);
                    }
                }
                else
                {
                    result.Objekt = Text.FindString(objektText,
                        (settings.Objekt.Start_FirstWord ? Environment.NewLine : "") + settings.Objekt.Start,
                        settings.Objekt.Stop);
                    result.Objekt = Text.FindString(result.Objekt, null, Environment.NewLine);
                }
            }

            #endregion

            #region Abschnitt

            if (!string.IsNullOrEmpty(settings.Abschnitt.Start))
            {
                var rowNumberStart = Text.FindString(settings.Abschnitt.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Abschnitt.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Abschnitt = Text.FindLine(abschnittText, rowNumberStart,
                            Text.FindString(settings.Abschnitt.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Abschnitt = Text.FindLine(abschnittText, rowNumberStart);
                        result.Abschnitt = Text.FindString(result.Abschnitt, null, settings.Abschnitt.Stop);
                    }
                }
                else
                {
                    result.Abschnitt = Text.FindString(abschnittText,
                        (settings.Abschnitt.Start_FirstWord ? Environment.NewLine : "") + settings.Abschnitt.Start,
                        settings.Abschnitt.Stop);
                    result.Abschnitt = Text.FindString(result.Abschnitt, null, Environment.NewLine);
                }

                //Filter Abschnitt falls gleich mit Straße
                if (result.Abschnitt != null && result.Abschnitt == result.Straße)
                {
                    result.Abschnitt = null;
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "abschnitt -> delete");
                }
            }

            #endregion

            #region Kreuzung

            if (!string.IsNullOrEmpty(settings.Kreuzung.Start))
            {
                var rowNumberStart = Text.FindString(settings.Kreuzung.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Kreuzung.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Kreuzung = Text.FindLine(kreuzungText, rowNumberStart,
                            Text.FindString(settings.Kreuzung.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Kreuzung = Text.FindLine(kreuzungText, rowNumberStart);
                        result.Kreuzung = Text.FindString(result.Kreuzung, null, settings.Kreuzung.Stop);
                    }
                }
                else
                {
                    result.Kreuzung = Text.FindString(kreuzungText,
                        (settings.Kreuzung.Start_FirstWord ? Environment.NewLine : "") + settings.Kreuzung.Start,
                        settings.Kreuzung.Stop);
                    result.Kreuzung = Text.FindString(result.Kreuzung, null, Environment.NewLine);
                }
            }

            #endregion

            #region Station

            if (!string.IsNullOrEmpty(settings.Station.Start))
            {
                var rowNumberStart = Text.FindString(settings.Station.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Station.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Station = Text.FindLine(stationText, rowNumberStart,
                            Text.FindString(settings.Station.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Station = Text.FindLine(stationText, rowNumberStart);
                        result.Station = Text.FindString(result.Station, null, settings.Station.Stop);
                    }
                }
                else
                {
                    result.Station = Text.FindString(stationText,
                        (settings.Station.Start_FirstWord ? Environment.NewLine : "") + settings.Station.Start,
                        settings.Station.Stop);
                    result.Station = Text.FindString(result.Station, null, Environment.NewLine);
                }
            }

            #endregion

            #region Schlagwort

            if (!string.IsNullOrEmpty(settings.Schlagwort.Start))
            {
                var rowNumberStart = Text.FindString(settings.Schlagwort.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Schlagwort.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Schlagwort = Text.FindLine(schlagwortText, rowNumberStart,
                            Text.FindString(settings.Schlagwort.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Schlagwort = Text.FindLine(schlagwortText, rowNumberStart);
                        result.Schlagwort = Text.FindString(result.Schlagwort, null, settings.Schlagwort.Stop);
                    }
                }
                else
                {
                    result.Schlagwort = Text.FindString(schlagwortText,
                        (settings.Schlagwort.Start_FirstWord ? Environment.NewLine : "") + settings.Schlagwort.Start,
                        settings.Schlagwort.Stop);
                    result.Schlagwort = Text.FindString(result.Schlagwort, null, Environment.NewLine);
                }
            }

            #endregion

            #region Stichwort

            if (!string.IsNullOrEmpty(settings.Stichwort1.Start))
            {
                var rowNumberStart = Text.FindString(settings.Stichwort1.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Stichwort1.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Stichwort = Text.FindLine(stichwort1Text, rowNumberStart,
                            Text.FindString(settings.Stichwort1.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Stichwort = Text.FindLine(stichwort1Text, rowNumberStart);
                        result.Stichwort = Text.FindString(result.Stichwort, null, settings.Stichwort1.Stop);
                    }
                }
                else
                {
                    result.Stichwort = Text.FindString(stichwort1Text,
                        (settings.Stichwort1.Start_FirstWord ? Environment.NewLine : "") + settings.Stichwort1.Start,
                        settings.Stichwort1.Stop);
                    result.Stichwort = Text.FindString(result.Stichwort, null, Environment.NewLine);
                    result.Marker = 1;
                }
            }

            if (string.IsNullOrEmpty(result.Stichwort) && !string.IsNullOrEmpty(settings.Stichwort2.Start))
            {
                var rowNumberStart = Text.FindString(settings.Stichwort2.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Stichwort2.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Stichwort = Text.FindLine(stichwort2Text, rowNumberStart,
                            Text.FindString(settings.Stichwort2.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Stichwort = Text.FindLine(stichwort2Text, rowNumberStart);
                        result.Stichwort = Text.FindString(result.Stichwort, null, settings.Stichwort2.Stop);
                    }
                }
                else
                {
                    result.Stichwort = Text.FindString(stichwort2Text,
                        (settings.Stichwort2.Start_FirstWord ? Environment.NewLine : "") + settings.Stichwort2.Start,
                        settings.Stichwort2.Stop);
                    result.Stichwort = Text.FindString(result.Stichwort, null, Environment.NewLine);
                    result.Marker = 2;
                }
            }

            if (string.IsNullOrEmpty(result.Stichwort) && !string.IsNullOrEmpty(settings.Stichwort3.Start))
            {
                var rowNumberStart = Text.FindString(settings.Stichwort3.Start, "%ZEILE-", "%");
                if (rowNumberStart != null)
                {
                    var rowNumberStop = Text.FindString(settings.Stichwort3.Stop, "%ZEILE-", "%");
                    if (rowNumberStop != null)
                    {
                        result.Stichwort = Text.FindLine(stichwort3Text, rowNumberStart,
                            Text.FindString(settings.Stichwort3.Stop, "%ZEILE-", "%"));
                    }
                    else
                    {
                        result.Stichwort = Text.FindLine(stichwort3Text, rowNumberStart);
                        result.Stichwort = Text.FindString(result.Stichwort, null, settings.Stichwort3.Stop);
                    }
                }
                else
                {
                    result.Stichwort = Text.FindString(stichwort3Text,
                        (settings.Stichwort3.Start_FirstWord ? Environment.NewLine : "") + settings.Stichwort3.Start,
                        settings.Stichwort3.Stop);
                    result.Stichwort = Text.FindString(result.Stichwort, null, Environment.NewLine);
                    result.Marker = 3;
                }
            }

            #endregion

            #endregion

            #region Filter "vor Anzeige"

            foreach (var filter in _filterList.Where(f => f.DoBeforeShow))
            {
                if (string.IsNullOrWhiteSpace(filter.SearchExpression)) continue;

                switch (filter.Field.Id)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(result.Bemerkung))
                            result.Bemerkung = result.Bemerkung
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Ort))
                            result.Ort = result.Ort.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Straße))
                            result.Straße = result.Straße.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Hausnummer))
                            result.Hausnummer = result.Hausnummer
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Objekt))
                            result.Objekt = result.Objekt.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Abschnitt))
                            result.Abschnitt = result.Abschnitt
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Kreuzung))
                            result.Kreuzung = result.Kreuzung.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Station))
                            result.Station = result.Station.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Schlagwort))
                            result.Schlagwort = result.Schlagwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Stichwort) && result.Marker == 1)
                            result.Stichwort = result.Stichwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Stichwort) && result.Marker == 2)
                            result.Stichwort = result.Stichwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        if (!string.IsNullOrEmpty(result.Stichwort) && result.Marker == 3)
                            result.Stichwort = result.Stichwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                    case 2:
                        //Einsatzmittel not possible
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(result.Bemerkung))
                            result.Bemerkung = result.Bemerkung
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                    case 4:
                        if (!string.IsNullOrEmpty(result.Ort))
                            result.Ort = result.Ort.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        break;
                    case 5:
                        if (!string.IsNullOrEmpty(result.Straße))
                            result.Straße = result.Straße.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        break;
                    case 6:
                        if (!string.IsNullOrEmpty(result.Hausnummer))
                            result.Hausnummer = result.Hausnummer
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                    case 7:
                        if (!string.IsNullOrEmpty(result.Objekt))
                            result.Objekt = result.Objekt.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        break;
                    case 8:
                        if (!string.IsNullOrEmpty(result.Abschnitt))
                            result.Abschnitt = result.Abschnitt
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                    case 9:
                        if (!string.IsNullOrEmpty(result.Kreuzung))
                            result.Kreuzung = result.Kreuzung.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        break;
                    case 10:
                        if (!string.IsNullOrEmpty(result.Station))
                            result.Station = result.Station.Replace(filter.SearchExpression, filter.ReplaceExpression)
                                .TrimStart(' ').TrimEnd(' ');

                        break;
                    case 11:
                        if (!string.IsNullOrEmpty(result.Schlagwort))
                            result.Schlagwort = result.Schlagwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                    case 12:
                        if (!string.IsNullOrEmpty(result.Stichwort) && result.Marker == 1)
                            result.Stichwort = result.Stichwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                    case 13:
                        if (!string.IsNullOrEmpty(result.Stichwort) && result.Marker == 2)
                            result.Stichwort = result.Stichwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                    case 14:
                        if (!string.IsNullOrEmpty(result.Stichwort) && result.Marker == 3)
                            result.Stichwort = result.Stichwort
                                .Replace(filter.SearchExpression, filter.ReplaceExpression).TrimStart(' ').TrimEnd(' ');

                        break;
                }
            }

            #endregion

            #region AAO

            var aaoExpressionValidList = new List<Aao>();
            var aaoResultList = new List<Aao>();

            //Check all Expression
            foreach (var aao in _aaoList)
                switch (aao.Condition.Id)
                {
                    case 1: //Schlagwort enthält
                        if (Text.CheckString(result.Schlagwort, aao.Expression)) aaoExpressionValidList.Add(aao);

                        break;
                    case 2: //Stichwort enthält
                        if (Text.CheckString(result.Stichwort, aao.Expression)) aaoExpressionValidList.Add(aao);

                        break;
                    case 3: //Alarmiertes Fahrzeug  
                        foreach (var vehicle in einsatzmittelFax)
                            if (Text.CheckString(vehicle.Name, aao.Expression) ||
                                Text.CheckString(vehicle.ViewText, aao.Expression) ||
                                Text.CheckString(vehicle.FaxText, aao.Expression))
                                aaoExpressionValidList.Add(aao);

                        break;
                    case 4: //Einsatzort ist
                        if (Text.CheckString(result.Ort, aao.Expression) ||
                            Text.CheckString(result.Straße, aao.Expression) ||
                            Text.CheckString(result.Objekt, aao.Expression))
                            aaoExpressionValidList.Add(aao);

                        break;
                    case 5: //Einsatzort ist nicht  
                        if (!Text.CheckString(result.Ort, aao.Expression) &&
                            !Text.CheckString(result.Straße, aao.Expression) &&
                            !Text.CheckString(result.Objekt, aao.Expression))
                            aaoExpressionValidList.Add(aao);

                        break;
                }

            //Check Combination
            foreach (var aao in aaoExpressionValidList)
                if (aao.Combination == null ||
                    aaoExpressionValidList.Where(a => a.Id == aao.Combination.Id).FirstOrDefault() != null)
                    aaoResultList.Add(aao);

            //Add Vehicles from aao to einsatz        
            foreach (var aao in aaoResultList)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"AAO [{aao.Name}] -> valid");

                foreach (var aaoVehicle in aao.Vehicles.OrderBy(v => v.Position).ToList())
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"AAO [{aao.Name}] -> {aaoVehicle.Vehicle.Name}");

                    if (result.Einsatzmittel.Where(v => v.Id == aaoVehicle.Vehicle.Id).FirstOrDefault() == null)
                        result.Einsatzmittel.Add(aaoVehicle.Vehicle);
                }
            }

            //Add Vehicles from fax to einsatz
            foreach (var vehicle in einsatzmittelFax)
                if (result.Einsatzmittel.Where(v => v.Id == vehicle.Id).FirstOrDefault() == null)
                    result.Einsatzmittel.Add(vehicle);

            #endregion

            if (result.IsValid() == false)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Result -> all fields empty is not valid");
                return null;
            }

            if (result.AbsenderValid == false)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Result -> Absender not valid");
                return null;
            }

            //Log values 
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Result -> {result}");
            return result;
        }

        #endregion //Private Funtions 
    }
}