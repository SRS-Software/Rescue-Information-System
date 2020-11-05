#region

using System.Reflection;
using GalaSoft.MvvmLight.Ioc;
using RIS.ViewModels;
using SRS.Utilities;

#endregion

namespace RIS.Factories
{
    public class ViewModelFactory
    {
        public static void Register()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register");

            SimpleIoc.Default.Register<MainPagersViewModel>(true);
            SimpleIoc.Default.Register<MainTickerViewModel>(true);
            SimpleIoc.Default.Register<MainVehiclesViewModel>(true);
            SimpleIoc.Default.Register<MainWarnweatherViewModel>(true);
            SimpleIoc.Default.Register<MainRiverlevelViewModel>(true);
            SimpleIoc.Default.Register<MainWebbrowserViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>(true);
        }

        public static void Initialize()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Initialize");

            SimpleIoc.Default.GetInstance<MainPagersViewModel>().Initialize();
            SimpleIoc.Default.GetInstance<MainTickerViewModel>().Initialize();
            SimpleIoc.Default.GetInstance<MainVehiclesViewModel>().Initialize();
            SimpleIoc.Default.GetInstance<MainWarnweatherViewModel>().Initialize();
            SimpleIoc.Default.GetInstance<MainRiverlevelViewModel>().Initialize();
            SimpleIoc.Default.GetInstance<MainWebbrowserViewModel>().Initialize();
            SimpleIoc.Default.GetInstance<MainViewModel>().Initialize();
        }
    }
}