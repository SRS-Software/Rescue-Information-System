#region

using System.Collections.Generic;
using System.Windows.Media;

#endregion

namespace RIS.Core.Alarmapp
{
    public class AlarmStatus
    {
        public enum UserStatus
        {
            Alarmed,
            Accpeted,
            Rejected
        }

        public int AlarmedUser { get; set; }
        public int AccpetedUser { get; set; }
        public int RejectedUser { get; set; }

        public List<Functiongroup> Functiongroups { get; set; } = new List<Functiongroup>();
        public List<User> Users { get; set; } = new List<User>();

        public static SolidColorBrush ConvertStringToBrush(string color)
        {
            var _brush = Brushes.Transparent;
            if (color != null) _brush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(color.ToUpper()));

            return _brush;
        }

        public static UserStatus ConvertStringToUserStatus(int status)
        {
            // status: 0=No / 1= Accepted / 2=Rejected
            switch (status)
            {
                case 1:
                    return UserStatus.Accpeted;
                case 2:
                    return UserStatus.Rejected;
                default:
                    return UserStatus.Alarmed;
            }
        }

        public class Functiongroup
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public SolidColorBrush Background { get; set; }
            public SolidColorBrush Foreground { get; set; }
            public int UserCount { get; set; }
        }

        public class User
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public UserStatus Status { get; set; }

            public SolidColorBrush StatusColor
            {
                get
                {
                    switch (Status)
                    {
                        case UserStatus.Accpeted:
                            return Brushes.Green;
                        case UserStatus.Rejected:
                            return Brushes.Red;
                        default:
                            return Brushes.Gray;
                    }
                }
            }

            public List<Functiongroup> Functiongroups { get; set; } = new List<Functiongroup>();
        }
    }
}