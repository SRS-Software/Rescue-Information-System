#region

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reflection;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using RIS.Business;
using RIS.Core;
using RIS.Core.Decoder;
using RIS.Core.Helper;
using RIS.Properties;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class MainPagersViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly IDecoderService decoderService;

        public MainPagersViewModel()
        {
            try
            {
                business = ServiceLocator.Current.GetInstance<IBusiness>();
                decoderService = ServiceLocator.Current.GetInstance<IDecoderService>();
                if (decoderService != null)
                    decoderService.PagerMessageReceived += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        decoderService_MessagePagerReceived(sender, e);
                    });

                //Load alarm list 
                PagerMessages =
                    Serializer.DeserializeFromFile<ObservableCollection<PagerViewModel>>(App.Path_DataAlarms);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        ~MainPagersViewModel()
        {
            try
            {
                if (decoderService != null)
                    decoderService.PagerMessageReceived -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        decoderService_MessagePagerReceived(sender, e);
                    });

                //Save alarm list 
                Serializer.SerializeToFile(PagerMessages, App.Path_DataAlarms);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Private Functions

        private void decoderService_MessagePagerReceived(object sender, PagerMessageEventArgs e)
        {
            if (e == null || e.Pager == null) return;

            var _pagerVM = new PagerViewModel
            {
                Time = e.Time,
                Identifier = e.Pager.Identifier,
                Name = e.Pager.Name,
                Priority = e.Pager.Priority
            };

            if (Settings.Default.Pagers_InsertItem)
                PagerMessages.Insert(0, _pagerVM);
            else
                PagerMessages.Add(_pagerVM);

            RaisePropertyChanged(() => PagerMessages);
        }

        #endregion //Private Funtions

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public Color ForegroundColor => Settings.Default.ForegroundColor;

        public bool AutoScrollToEnd => !Settings.Default.Pagers_InsertItem;

        private ObservableCollection<PagerViewModel> pagerMessages = new ObservableCollection<PagerViewModel>();

        public ObservableCollection<PagerViewModel> PagerMessages
        {
            get => pagerMessages;
            set
            {
                if (pagerMessages == value) return;

                pagerMessages = value;
                RaisePropertyChanged(() => PagerMessages);
            }
        }

        #endregion //Public Properties

        #region Public Functions

        public void Initialize()
        {
            RaisePropertyChanged(() => ForegroundColor);
            RaisePropertyChanged(() => AutoScrollToEnd);
        }

        public void Reset()
        {
            //Clear List
            PagerMessages.Clear();
            RaisePropertyChanged(() => PagerMessages);

            //Save alarm list 
            Serializer.SerializeToFile(PagerMessages, App.Path_DataAlarms);
        }

        #endregion //Public Functions

        #region Private Properties

        #endregion //Private Properties
    }
}