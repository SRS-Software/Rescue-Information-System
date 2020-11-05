#region

using System.Windows.Media;
using GalaSoft.MvvmLight;

#endregion

namespace RIS.ViewModels
{
    public class AlarmappGroupViewModel : ViewModelBase
    {
        private SolidColorBrush _groupBackground;

        private SolidColorBrush _groupForeground;

        private string _groupName;

        private int _groupUserCount;
        public string GroupId { get; set; }

        public string GroupName
        {
            get => _groupName;
            set
            {
                if (_groupName == value) return;

                _groupName = value;

                RaisePropertyChanged(() => GroupName);
            }
        }

        public int GroupUserCount
        {
            get => _groupUserCount;
            set
            {
                if (_groupUserCount == value) return;

                _groupUserCount = value;

                RaisePropertyChanged(() => GroupUserCount);
            }
        }

        public SolidColorBrush GroupForeground
        {
            get => _groupForeground;
            set
            {
                if (_groupForeground == value) return;

                _groupForeground = value;
                RaisePropertyChanged(() => GroupForeground);
            }
        }

        public SolidColorBrush GroupBackground
        {
            get => _groupBackground;
            set
            {
                if (_groupBackground == value) return;

                _groupBackground = value;
                RaisePropertyChanged(() => GroupBackground);
            }
        }
    }

    public class AlarmappOverviewGroupViewModel : AlarmappGroupViewModel
    {
        private int _row;

        public AlarmappOverviewGroupViewModel(int row, string groupId, string groupName, int groupUserCount,
            SolidColorBrush groupForeground, SolidColorBrush groupBackground)
        {
            Row = row;
            GroupId = groupId;
            GroupName = groupName;
            GroupUserCount = groupUserCount;
            GroupForeground = groupForeground;
            GroupBackground = groupBackground;
        }

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
    }

    public class AlarmappUserGroupViewModel : AlarmappGroupViewModel
    {
        private int _column;

        public AlarmappUserGroupViewModel(int column, string groupName, SolidColorBrush groupForeground,
            SolidColorBrush groupBackground)
        {
            Column = column;
            GroupName = groupName;
            GroupForeground = groupForeground;
            GroupBackground = groupBackground;
        }

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
    }
}