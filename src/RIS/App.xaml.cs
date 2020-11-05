#region

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using RIS.Core.Helper;
using RIS.Factories;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS
{
    public partial class App
    {
        public static readonly string Path_DataAlarms = Settings.Default.WorkingFolder + "\\DataAlarms.csv";
        public static readonly string Path_DataVehicles = Settings.Default.WorkingFolder + "\\DataVehicles.csv";

        public static string Path_Record;
        public static string Path_Temp;
        public static string Path_Log;

        public App()
        {
            InitializeComponent();

            // Set shutdown mode to manual
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                //Process command line args
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-DeleteConfig":
                            var config =
                                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel
                                    .PerUserRoamingAndLocal);
                            var directoryInfo = new DirectoryInfo(config.FilePath);
                            directoryInfo.Parent?.Parent?.Delete(true);

                            MessageBox.Show("Einstellungsdatei wurde gelöscht", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            break;
                        case "-ResetPassword":
                            Settings.Default.AdminPassword = null;
                            Settings.Default.Save();

                            MessageBox.Show("Admin-Passwort wurde gelöscht", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            break;
                    }

                    Current.Shutdown();
                }

                //Check if process is already running
                var countAppProcess = Process.GetProcesses()
                    .Count(p => p.ProcessName == Process.GetCurrentProcess().ProcessName);
                if (countAppProcess > 1)
                {
                    MessageBox.Show("RIS läuft bereits", MessageBoxButton.OK, MessageBoxImage.Information);
                    Current.Shutdown();
                }

                //Create Application and show
                var application = new App();
                application.InitializeComponent();
                application.Run();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Add the event for catching unhandled exceptions on the thread attached to the specific Dispatcher
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            //Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Update Settings if new version
            var _settingsUpdated = false;
            if (!Settings.Default.SettingsUpdated)
            {
                Settings.Default.Upgrade();
                Settings.Default.Reload();
                Settings.Default.SettingsUpdated = true;
                Settings.Default.Save();
                _settingsUpdated = true;
            }

            //Set WorkingFolder  
            Settings.Default.WorkingFolder =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RIS");
            Settings.Default.Save();
            if (!Directory.Exists(Settings.Default.WorkingFolder))
                Directory.CreateDirectory(Settings.Default.WorkingFolder);

            //Check rights on working folder
            try
            {
                var accessRights = new UserAccessRights(Settings.Default.WorkingFolder);
                if (!accessRights.CanModify)
                {
                    MessageBox.Show(
                        $"Kein Zugriff auf den Ordner '{Settings.Default.WorkingFolder}'.\r\nBitte Überpürfen Sie Ihre Berechtigungen.",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown(-1);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                Current.Shutdown(-1);
            }

            //Initialize logger with path 
            Path_Log = Path.Combine(Settings.Default.WorkingFolder, "Log");
            Logger.Initialize(Path_Log);
            Logger.WriteDebug(
                $"{Assembly.GetEntryAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version} -> start");
            Logger.SetLevel(Settings.Default.LogLevel);
            Logger.WriteDebug("RIS: WorkingFolder -> " + Settings.Default.WorkingFolder);

            //MVVM Light Messenger
            DispatcherHelper.Initialize();

            //Register Locators and Factories
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            ServiceFactory.Register();
            ViewModelFactory.Register();
            ViewFactory.Register();

            //Show start window  
            ViewFactory.StartSplashScreen.Show();

            //Initialize Factories
            ServiceFactory.Initialize();
            ViewModelFactory.Initialize();
            ViewFactory.Initialize();

            //Folder Temp
            Path_Temp = Path.Combine(Settings.Default.WorkingFolder, "Temp");
            if (!Directory.Exists(Path_Temp))
            {
                Directory.CreateDirectory(Path_Temp);
                Logger.WriteDebug("RIS: Ordner für Temp-Files wurde erstellt -> " + Path_Temp);
            }

            foreach (var _tempFile in new DirectoryInfo(Path_Temp).GetFiles())
            {
                _tempFile.Delete();
                Logger.WriteDebug("RIS: Delete -> " + _tempFile);
            }

            //Folder Funkaufzeichnung
            Path_Record = Path.Combine(Settings.Default.WorkingFolder, "Record");
            if (!Directory.Exists(Path_Record))
            {
                Directory.CreateDirectory(Path_Record);
                Logger.WriteDebug("RIS: Ordner für Funkaufzeichnung wurde erstellt -> " + Path_Record);
            }

            //Delete old data on update
            if (_settingsUpdated)
            {
                //Delete all old data
                if (File.Exists(Path_DataVehicles))
                {
                    File.Delete(Path_DataVehicles);
                    Logger.WriteDebug("RIS: Delete -> " + Path_DataVehicles);
                }

                if (File.Exists(Path_DataAlarms))
                {
                    File.Delete(Path_DataAlarms);
                    Logger.WriteDebug("RIS: Delete -> " + Path_DataAlarms);
                }

                foreach (var _logFile in new DirectoryInfo(Path_Log).GetFiles())
                    try
                    {
                        _logFile.Delete();
                        Logger.WriteDebug("RIS: Delete -> " + _logFile);
                    }
                    catch (IOException ex)
                    {
                        Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                    }
            }

            //Start all services
            ServiceFactory.Start();

            //Show Window
            MainWindow = new MainWindow();
            MainWindow.Show();

            //Close StartScreen
            ViewFactory.StartSplashScreen.Close();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;
            Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
        }
    }
}