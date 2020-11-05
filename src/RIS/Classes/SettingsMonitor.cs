#region

using System;
using System.Collections.ObjectModel;

#endregion

namespace RIS
{
    public class SettingsMonitor
    {
        public SettingsMonitor()
        {
            Monday = new ObservableCollection<MonitorItem>();
            Tuesday = new ObservableCollection<MonitorItem>();
            Wednesday = new ObservableCollection<MonitorItem>();
            Thursday = new ObservableCollection<MonitorItem>();
            Friday = new ObservableCollection<MonitorItem>();
            Saturday = new ObservableCollection<MonitorItem>();
            Sunday = new ObservableCollection<MonitorItem>();
        }

        public ObservableCollection<MonitorItem> Monday { get; set; }
        public ObservableCollection<MonitorItem> Tuesday { get; set; }
        public ObservableCollection<MonitorItem> Wednesday { get; set; }
        public ObservableCollection<MonitorItem> Thursday { get; set; }
        public ObservableCollection<MonitorItem> Friday { get; set; }
        public ObservableCollection<MonitorItem> Saturday { get; set; }
        public ObservableCollection<MonitorItem> Sunday { get; set; }

        public class MonitorItem
        {
            public MonitorItem()
            {
                //Needed for serialization
            }

            public MonitorItem(DateTime _start, DateTime _stop)
            {
                Start = _start;
                Stop = _stop;
            }

            public DateTime Start { get; set; }
            public DateTime Stop { get; set; }
        }
    }
}