#region

using System.Windows.Media;

#endregion

namespace RIS
{
    public class SettingsFms
    {
        public SettingsFms()
        {
            Status1 = new FmsItem();
            Status2 = new FmsItem();
            Status3 = new FmsItem();
            Status4 = new FmsItem();
            Status5 = new FmsItem();
            Status6 = new FmsItem();
            Status7 = new FmsItem();
            Status8 = new FmsItem();
            Status9 = new FmsItem();

            StatusA = new FmsItem();
            StatusC = new FmsItem();
            StatusE = new FmsItem();
            StatusF = new FmsItem();
            StatusH = new FmsItem();
            StatusJ = new FmsItem();
            StatusL = new FmsItem();
            StatusP = new FmsItem();
            StatusU = new FmsItem();
        }

        public FmsItem Status1 { get; set; }
        public FmsItem Status2 { get; set; }
        public FmsItem Status3 { get; set; }
        public FmsItem Status4 { get; set; }
        public FmsItem Status5 { get; set; }
        public FmsItem Status6 { get; set; }
        public FmsItem Status7 { get; set; }
        public FmsItem Status8 { get; set; }
        public FmsItem Status9 { get; set; }

        public FmsItem StatusA { get; set; }
        public FmsItem StatusC { get; set; }
        public FmsItem StatusE { get; set; }
        public FmsItem StatusF { get; set; }
        public FmsItem StatusH { get; set; }
        public FmsItem StatusJ { get; set; }
        public FmsItem StatusL { get; set; }
        public FmsItem StatusP { get; set; }
        public FmsItem StatusU { get; set; }

        public class FmsItem
        {
            private SolidColorBrush color;
            public bool MainOn { get; set; }
            public bool AlarmOn { get; set; }

            public SolidColorBrush Color
            {
                get => color ?? new SolidColorBrush();
                set => color = value;
            }
        }
    }
}