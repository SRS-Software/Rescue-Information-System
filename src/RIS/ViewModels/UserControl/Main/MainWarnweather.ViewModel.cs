#region

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using RIS.Core;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class MainWarnweatherViewModel : ViewModelBase
    {
        private readonly IWarnweatherService warnweatherService;

        public MainWarnweatherViewModel()
        {
            try
            {
                warnweatherService = ServiceLocator.Current.GetInstance<IWarnweatherService>();
                if (warnweatherService != null)
                    warnweatherService.ImageReceived += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        WarnweatherService_ImageReceived(sender, e);
                    });
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Properties

        public byte[] Image { get; set; }

        #endregion //Public Properties

        ~MainWarnweatherViewModel()
        {
            try
            {
                if (warnweatherService != null)
                    warnweatherService.ImageReceived -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        WarnweatherService_ImageReceived(sender, e);
                    });
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Functions

        public void Initialize()
        {
            try
            {
                //Reset all values
                Image = null;

                //Load logo as image
                using (var _memoryStream = new MemoryStream())
                {
                    var _resourceStream =
                        Application.GetResourceStream(
                            new Uri(@"Resources/Image.NoData.png", UriKind.RelativeOrAbsolute));
                    _resourceStream.Stream.CopyTo(_memoryStream);

                    Image = _memoryStream.ToArray();
                    RaisePropertyChanged(() => Image);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #endregion //Public Functions

        #region Private Functions

        private void WarnweatherService_ImageReceived(object sender, byte[] e)
        {
            if (e != null)
            {
                Image = e;
                RaisePropertyChanged(() => Image);
            }
        }

        #endregion //Private Funtions

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Private Properties

        #endregion //Private Properties
    }
}