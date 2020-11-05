#region

using System;

#endregion

namespace RIS.ViewModels
{
    public class PagerViewModel
    {
        public DateTime Time { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public bool Priority { get; set; }
    }
}