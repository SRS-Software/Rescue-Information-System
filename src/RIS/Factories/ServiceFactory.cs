#region

using System;
using System.Reflection;
using GalaSoft.MvvmLight.Ioc;
using RIS.Business;
using RIS.Core;
using SRS.Utilities;

#endregion

namespace RIS.Factories
{
    public class ServiceFactory
    {
        public static void Register()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register");

            SimpleIoc.Default.Register<IBusiness, DataBusiness>(true);
            SimpleIoc.Default.Register<IMouseService, MouseService>(true);
            SimpleIoc.Default.GetInstance<IMouseService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IRiverlevelService, RiverlevelService>(true);
            SimpleIoc.Default.GetInstance<IRiverlevelService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IWarnweatherService, WarnweatherService>(true);
            SimpleIoc.Default.GetInstance<IWarnweatherService>().ExceptionOccured += Service_ExceptionOccured;

            SimpleIoc.Default.Register<IDecoderService, DecoderService>(true);
            SimpleIoc.Default.GetInstance<IDecoderService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IFaxService, FaxService>(true);
            SimpleIoc.Default.GetInstance<IFaxService>().ExceptionOccured += Service_ExceptionOccured;
            
            SimpleIoc.Default.Register<IWatchdogService, WatchdogService>(true);
            SimpleIoc.Default.GetInstance<IWatchdogService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IAmsService, AmsService>(true);
            SimpleIoc.Default.GetInstance<IAmsService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IAlarmappService, AlarmappService>(true);
            SimpleIoc.Default.GetInstance<IAlarmappService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IFireboardService, FireboardService>(true);
            SimpleIoc.Default.GetInstance<IFireboardService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IMailService, MailService>(true);
            SimpleIoc.Default.GetInstance<IMailService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IMapService, MapService>(true);
            SimpleIoc.Default.GetInstance<IMapService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IPrinterService, PrinterService>(true);
            SimpleIoc.Default.GetInstance<IPrinterService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IMonitorService, MonitorService>(true);
            SimpleIoc.Default.GetInstance<IMonitorService>().ExceptionOccured += Service_ExceptionOccured;
            SimpleIoc.Default.Register<IRebootService, RebootService>(true);
            SimpleIoc.Default.GetInstance<IRebootService>().ExceptionOccured += Service_ExceptionOccured;
        }

        private static void Service_ExceptionOccured(object sender, ExceptionEventArgs e)
        {
            if (e == null || e.Methode == null)
                return;

            if (e.Message != null)
                Logger.WriteError(e.Methode, e.Message);
            if (e.Error != null)
                Logger.WriteError(e.Methode, e.Error);
        }

        public static void Initialize()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Initialize");

            //Migrate DB if necessary
            SimpleIoc.Default.GetInstance<IBusiness>().CheckConnection();
        }

        public static void Start()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Start");

            SimpleIoc.Default.GetInstance<IMouseService>().Start();
            SimpleIoc.Default.GetInstance<IRiverlevelService>().Start();
            SimpleIoc.Default.GetInstance<IWarnweatherService>().Start();

            SimpleIoc.Default.GetInstance<IDecoderService>().Start();
            SimpleIoc.Default.GetInstance<IFaxService>().Start();
            SimpleIoc.Default.GetInstance<IFireboardService>().Start();

            SimpleIoc.Default.GetInstance<IAmsService>().Start();
            SimpleIoc.Default.GetInstance<IMailService>().Start();
            SimpleIoc.Default.GetInstance<IMapService>().Start();
            SimpleIoc.Default.GetInstance<IAlarmappService>().Start();

            SimpleIoc.Default.GetInstance<IWatchdogService>().Start();
            SimpleIoc.Default.GetInstance<IPrinterService>().Start();
            SimpleIoc.Default.GetInstance<IMonitorService>().Start();
            SimpleIoc.Default.GetInstance<IRebootService>().Start();
        }

        public static void Stop()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Stop");

            SimpleIoc.Default.GetInstance<IMouseService>().Stop();
            SimpleIoc.Default.GetInstance<IRiverlevelService>().Stop();
            SimpleIoc.Default.GetInstance<IWarnweatherService>().Stop();

            SimpleIoc.Default.GetInstance<IDecoderService>().Stop();
            SimpleIoc.Default.GetInstance<IFaxService>().Stop();
            SimpleIoc.Default.GetInstance<IFireboardService>().Stop();

            SimpleIoc.Default.GetInstance<IAmsService>().Stop();
            SimpleIoc.Default.GetInstance<IMailService>().Stop();
            SimpleIoc.Default.GetInstance<IMapService>().Stop();
            SimpleIoc.Default.GetInstance<IAlarmappService>().Stop();

            SimpleIoc.Default.GetInstance<IWatchdogService>().Stop();
            SimpleIoc.Default.GetInstance<IPrinterService>().Stop();
            SimpleIoc.Default.GetInstance<IMonitorService>().Stop();
            SimpleIoc.Default.GetInstance<IRebootService>().Stop();
        }
    }
}