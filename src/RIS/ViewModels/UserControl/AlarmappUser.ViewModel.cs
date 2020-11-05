#region

using System.Collections.ObjectModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

#endregion

namespace RIS.ViewModels
{
    public class AlarmappUserViewModel : ViewModelBase
    {
        #region Private Fields

        private int _userGroupCurrent;

        #endregion

        public AlarmappUserViewModel(int row, int column, string userId, string userName, SolidColorBrush userStatus)
        {
            Row = row;
            Column = column;

            UserId = userId;
            UserName = userName;
            UserStatus = userStatus;
        }

        #region Public Properties

        private int _row;

        public int Row
        {
            get => _row;
            private set
            {
                if (_row == value) return;

                _row = value;
                RaisePropertyChanged(() => Row);
            }
        }

        private int _column;

        public int Column
        {
            get => _column;
            private set
            {
                if (_column == value) return;

                _column = value;
                RaisePropertyChanged(() => Column);
            }
        }

        public string UserId { get; }

        private int _userGroupCount;

        public int UserGroupCount
        {
            get => _userGroupCount;
            set
            {
                if (_userGroupCount == value) return;

                _userGroupCount = value;

                RaisePropertyChanged(() => UserGroupCount);
            }
        }

        private string _userName;

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName == value) return;

                _userName = value;
                RaisePropertyChanged(() => UserName);
            }
        }

        private SolidColorBrush _userStatus;

        public SolidColorBrush UserStatus
        {
            get => _userStatus;
            set
            {
                if (_userStatus == value) return;

                _userStatus = value;
                RaisePropertyChanged(() => UserStatus);
            }
        }

        public ObservableCollection<AlarmappUserGroupViewModel> UserGroupList { get; } =
            new ObservableCollection<AlarmappUserGroupViewModel>();

        #endregion //Public Properties

        #region Public Functions

        public void ClearFunctiongroups()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                UserGroupList.Clear();
                RaisePropertyChanged(() => UserGroupList);

                _userGroupCurrent = 0;
                _userGroupCount = 3;
            });
        }

        public void AddFunctiongroups(string groupName, SolidColorBrush groupForeground,
            SolidColorBrush groupBackground)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                UserGroupList.Add(new AlarmappUserGroupViewModel(_userGroupCurrent, groupName, groupForeground,
                    groupBackground));
                RaisePropertyChanged(() => UserGroupList);

                _userGroupCurrent++;
                if (_userGroupCurrent > UserGroupCount) UserGroupCount++;
            });
        }

        #endregion

        #region Private Functions

        #endregion //Private Functions
    }
}