#region

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using RIS.Core;
using RIS.Core.Fax;
using RIS.Core.Map;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class AlarmRouteViewModel : ViewModelBase
    {
        private readonly string einsatzGuid;
        private readonly IMapService mapService;

        public AlarmRouteViewModel(Einsatz _einsatz)
        {
            try
            {
                //Load logo as image
                using (var _memoryStream = new MemoryStream())
                {
                    var _resourceStream =
                        Application.GetResourceStream(new Uri(@"Resources/Image.Logo.png", UriKind.RelativeOrAbsolute));
                    _resourceStream.Stream.CopyTo(_memoryStream);
                    Image = _memoryStream.ToArray();
                }

                einsatzGuid = _einsatz.Guid;

                mapService = ServiceLocator.Current.GetInstance<IMapService>();
                if (mapService != null)
                    mapService.Finished += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        mapService_Finished(sender, e);
                    });
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public override void Cleanup()
        {
            if (mapService != null)
                mapService.Finished -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    mapService_Finished(sender, e);
                });

            base.Cleanup();
        }

        #region Private Functions

        private void mapService_Finished(object sender, FinishedEventArgs e)
        {
            if (e == null || e.ImageWindow == null || e.Einsatz == null || e.Einsatz.Guid != einsatzGuid) return;

            Image = e.ImageWindow;
        }

        #endregion //Private Funtions 

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        private byte[] image;

        public byte[] Image
        {
            get => image;
            set
            {
                if (image == value) return;

                image = value;

                RaisePropertyChanged(() => Image);
            }
        }

        #endregion //Public Properties

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        #endregion //Private Properties
    }
}