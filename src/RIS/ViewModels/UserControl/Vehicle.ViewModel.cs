#region

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using RIS.Core.Helper;
using RIS.Model;
using RIS.Properties;

#endregion

namespace RIS.ViewModels
{
    public class VehicleViewModel : ViewModelBase
    {
        private readonly SettingsFms _fmsSettings;

        public VehicleViewModel(Vehicle vehicle, int row, int column, bool isAlarm)
        {
            Vehicle = vehicle;
            Row = row;
            Column = column;
            IsAlarm = isAlarm;
            _fmsSettings = Serializer.Deserialize<SettingsFms>(Settings.Default.FmsSettings);

            ChangeStatusVisibility = new ObservableCollection<Visibility>
            {
                getStatusOn("1") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("2") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("3") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("4") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("5") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("6") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("7") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("8") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("9") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("A") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("C") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("E") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("F") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("H") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("J") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("L") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("P") ? Visibility.Visible : Visibility.Collapsed,
                getStatusOn("U") ? Visibility.Visible : Visibility.Collapsed
            };
        }

        #region Public Functions

        /// <summary>
        ///     Change Status of current vehicle item
        /// </summary>
        public bool ChangeStatus(string status)
        {
            if (getStatusOn(status) == false) return false;

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                StatusText = status;
                StatusColor = getStatusColor(status);
            });

            return true;
        }

        #endregion

        #region Commands

        private RelayCommand<string> changeStatusCommand;

        public RelayCommand<string> ChangeStatusCommand => changeStatusCommand ??
                                                           (changeStatusCommand =
                                                               new RelayCommand<string>(OnChangeStatus,
                                                                   CanChangeStatus));

        private bool CanChangeStatus(string _status)
        {
            if (string.IsNullOrWhiteSpace(_status)) return false;

            return true;
        }

        private void OnChangeStatus(string _status)
        {
            ChangeStatus(_status);
        }

        #endregion //Commands

        #region Public Properties

        public ObservableCollection<Visibility> ChangeStatusVisibility { get; }

        public Vehicle Vehicle { get; }

        public int Row { get; }

        public int Column { get; }

        public bool IsAlarm { get; }

        public string VehicleText => Vehicle.ViewText;

        public bool StatusOn => !string.IsNullOrEmpty(Vehicle.BosIdentifier);

        public bool BoxOn
        {
            get
            {
                if (IsAlarm) return Settings.Default.Vehicles_Statusbox;

                return true;
            }
        }

        public Visibility BoxVisibility
        {
            get
            {
                if (StatusOn && BoxOn) return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }

        private string statusText = "2";

        public string StatusText
        {
            get => statusText;
            private set
            {
                if (statusText == value) return;

                statusText = value;

                RaisePropertyChanged(() => StatusText);
            }
        }

        private SolidColorBrush statusColor;

        public SolidColorBrush StatusColor
        {
            get => statusColor;
            private set
            {
                if (statusColor == value) return;

                statusColor = value;

                RaisePropertyChanged(() => BoxColor);
                RaisePropertyChanged(() => ElementColor);
            }
        }

        public SolidColorBrush BoxColor
        {
            get
            {
                if (StatusOn && BoxOn) return statusColor;

                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public SolidColorBrush ElementColor
        {
            get
            {
                if (StatusOn && !BoxOn) return statusColor;

                return new SolidColorBrush(Colors.Transparent);
            }
        }

        #endregion //Public Properties

        #region Private Fields

        #endregion

        #region Private Functions

        private bool getStatusOn(string _status)
        {
            var _settingsItem = getSettingsItem(_status);
            if (_settingsItem == null) return false;

            return IsAlarm ? _settingsItem.AlarmOn : _settingsItem.MainOn;
        }

        private SolidColorBrush getStatusColor(string _status)
        {
            var _settingsItem = getSettingsItem(_status);
            if (_settingsItem == null) return new SolidColorBrush();

            return _settingsItem.Color;
        }

        private SettingsFms.FmsItem getSettingsItem(string _status)
        {
            if (_fmsSettings == null || string.IsNullOrWhiteSpace(_status)) return new SettingsFms.FmsItem();

            switch (_status)
            {
                case "1":
                    return _fmsSettings.Status1;
                case "2":
                    return _fmsSettings.Status2;
                case "3":
                    return _fmsSettings.Status3;
                case "4":
                    return _fmsSettings.Status4;
                case "5":
                    return _fmsSettings.Status5;
                case "6":
                    return _fmsSettings.Status6;
                case "7":
                    return _fmsSettings.Status7;
                case "8":
                    return _fmsSettings.Status8;
                case "9":
                    return _fmsSettings.Status9;

                case "A":
                    return _fmsSettings.StatusA;
                case "C":
                    return _fmsSettings.StatusC;
                case "E":
                    return _fmsSettings.StatusE;
                case "F":
                    return _fmsSettings.StatusF;
                case "H":
                    return _fmsSettings.StatusH;
                case "J":
                    return _fmsSettings.StatusJ;
                case "L":
                    return _fmsSettings.StatusL;
                case "P":
                    return _fmsSettings.StatusP;
                case "U":
                    return _fmsSettings.StatusU;

                default:
                    return new SettingsFms.FmsItem();
            }
        }

        #endregion //Private Functions
    }
}