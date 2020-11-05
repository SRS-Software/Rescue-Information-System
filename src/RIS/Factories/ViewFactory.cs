#region

using System.Reflection;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using RIS.Views;
using SRS.Utilities;

#endregion

namespace RIS.Factories
{
    public class ViewFactory
    {
        public static StartSplashScreen StartSplashScreen => ServiceLocator.Current.GetInstance<StartSplashScreen>();

        public static WaitSplashScreen WaitSplashScreen => ServiceLocator.Current.GetInstance<WaitSplashScreen>();

        public static void Register()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register");

            SimpleIoc.Default.Register<StartSplashScreen>(true);
            SimpleIoc.Default.Register<WaitSplashScreen>(true);
        }

        public static void Initialize()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Initialize");
        }
    }
}