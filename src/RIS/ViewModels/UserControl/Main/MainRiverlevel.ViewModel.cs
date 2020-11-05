#region

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using RIS.Core;
using RIS.Core.Helper;
using RIS.Core.Riverlevel;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class MainRiverlevelViewModel : ViewModelBase
    {
        private readonly IRiverlevelService riverlevelService;

        public MainRiverlevelViewModel()
        {
            try
            {
                riverlevelService = ServiceLocator.Current.GetInstance<IRiverlevelService>();
                if (riverlevelService != null)
                {
                    riverlevelService.DataReceived += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        RiverlevelService_DataReceived(sender, e);
                    });

                    riverlevelService.ImageReceived += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        RiverlevelService_ImageReceived(sender, e);
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        ~MainRiverlevelViewModel()
        {
            try
            {
                if (riverlevelService != null)
                {
                    riverlevelService.DataReceived -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        RiverlevelService_DataReceived(sender, e);
                    });

                    riverlevelService.ImageReceived -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        RiverlevelService_ImageReceived(sender, e);
                    });
                }
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
                Description = null;
                Warning = 0;
                Riverlevel_Description = null;
                Riverlevel_Value = null;
                Flowspeed_Description = null;
                Flowspeed_Value = null;
                DataDate = null;

                //Load logo as image
                using (var _memoryStream = new MemoryStream())
                {
                    var _resourceStream =
                        Application.GetResourceStream(
                            new Uri(@"Resources/Image.NoData.png", UriKind.RelativeOrAbsolute));
                    _resourceStream.Stream.CopyTo(_memoryStream);
                    Image = _memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #endregion //Public Functions

        #region Private Functions

        private void RiverlevelService_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Description = e.Description;
            RaisePropertyChanged(() => Description);

            Warning = Parser.StringToInt(e.Warning);
            RaisePropertyChanged(() => Warning);

            Riverlevel_Description = e.Riverlevel_Description;
            RaisePropertyChanged(() => Riverlevel_Description);

            Riverlevel_Value = e.Riverlevel_Value;
            RaisePropertyChanged(() => Riverlevel_Value);

            Flowspeed_Description = e.Flowspeed_Description;
            RaisePropertyChanged(() => Flowspeed_Description);

            Flowspeed_Value = e.Flowspeed_Value;
            RaisePropertyChanged(() => Flowspeed_Value);

            DataDate = e.DataDate;
            RaisePropertyChanged(() => DataDate);
        }

        private void RiverlevelService_ImageReceived(object sender, byte[] e)
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

        #region Public Properties

        public byte[] Image { get; set; }
        public string Description { get; set; }
        public int Warning { get; set; }
        public string Riverlevel_Description { get; set; }
        public string Riverlevel_Value { get; set; }
        public string Flowspeed_Description { get; set; }
        public string Flowspeed_Value { get; set; }
        public string DataDate { get; set; }

        #endregion //Public Properties

        #region Private Properties

        #endregion //Private Properties
    }
}