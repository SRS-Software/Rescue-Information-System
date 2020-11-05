#region

using System;
using System.Reflection;
using GalaSoft.MvvmLight;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class MainWebbrowserViewModel : ViewModelBase
    {
        public MainWebbrowserViewModel()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        ~MainWebbrowserViewModel()
        {
        }


        #region Public Functions

        public void Initialize()
        {
            try
            {
                //this.Source = Settings.Default.Webbrowser_Url.ToUri(); 
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #endregion //Public Functions


        #region Private Properties

        #endregion //Private Properties

        #region Private Functions

        //private void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(Settings.Default.Webbrowser_Url))
        //            this.Source = new Uri(Settings.Default.Webbrowser_Url);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
        //    }
        //    finally
        //    {
        //        //Restart timer
        //        _refreshTimer.Start();
        //    }
        //}

        #endregion //Private Funtions

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        private Uri _source;

        public Uri Source
        {
            get => _source;
            set
            {
                if (_source == value) return;

                _source = value;
                RaisePropertyChanged(() => Source);
            }
        }

        #endregion //Public Properties
    }
}