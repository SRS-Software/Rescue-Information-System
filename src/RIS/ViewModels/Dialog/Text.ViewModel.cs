#region

using System;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class TextViewModel : ViewModelBase
    {
        public TextViewModel()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Properties

        public string WindowTitel => "Rescue-Information-System";

        #endregion //Public Properties

        #region Commands

        private RelayCommand closeCommand;

        public RelayCommand CloseCommand
        {
            get
            {
                if (closeCommand == null) closeCommand = new RelayCommand(() => OnClose(), () => CanClose());

                return closeCommand;
            }
        }

        private bool CanClose()
        {
            return true;
        }

        private void OnClose()
        {
            Logger.WriteDebug("TextWindow: CloseCommand");

            RaiseCloseRequestEvent();
        }

        #endregion //Commands

        #region Events

        public event EventHandler CloseRequested;

        private void RaiseCloseRequestEvent()
        {
            var handler = CloseRequested;
            if (handler != null) handler(this, new EventArgs());
        }

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        #endregion //Private Properties

        #region Private Functions

        #endregion //Private Funtions
    }
}