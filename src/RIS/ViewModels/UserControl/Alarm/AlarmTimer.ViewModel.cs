#region

using System;
using System.Reflection;
using System.Timers;
using GalaSoft.MvvmLight;
using RIS.Core.Fax;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class AlarmTimerViewModel : ViewModelBase
    {
        private readonly string einsatzGuid;

        public AlarmTimerViewModel(Einsatz _einsatz)
        {
            try
            {
                einsatzGuid = _einsatz.Guid;
                alarmTime = _einsatz.AlarmTime;

                updateTimer = new Timer
                {
                    Interval = 1000,
                    AutoReset = false
                };
                updateTimer.Elapsed += updateTimer_Elapsed;
                updateTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public override void Cleanup()
        {
            if (updateTimer != null) updateTimer.Stop();

            base.Cleanup();
        }

        #region Private Functions

        private void updateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            TimerText = (DateTime.Now - alarmTime).ToString(@"hh\:mm\:ss");

            updateTimer.Start();
        }

        #endregion //Private Funtions

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        private string timerText;

        public string TimerText
        {
            get => timerText;
            set
            {
                if (timerText == value) return;

                timerText = value;
                RaisePropertyChanged(() => TimerText);
            }
        }

        #endregion //Public Properties

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        private Timer updateTimer { get; }
        private DateTime alarmTime { get; }

        #endregion //Private Properties
    }
}