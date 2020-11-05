#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RIS.Core.Decoder;

#endregion

namespace RIS.Core.Ams
{
    public class Einsatz
    {
        public Einsatz(string _recordPath)
        {
            FmsMessages = new List<FmsMessageEventArgs>();
            PagerMessages = new List<PagerMessageEventArgs>();

            AlarmTime = DateTime.Now;
            RecordPath = Path.Combine(_recordPath, "AMS_" + AlarmTime.ToString("yyyy-MM-dd_HH-mm-ss") + ".mp3");
        }

        public DateTime AlarmTime { get; set; }
        public string RecordPath { get; set; }

        public List<FmsMessageEventArgs> FmsMessages { get; }
        public List<PagerMessageEventArgs> PagerMessages { get; }

        public void AddFms(FmsMessageEventArgs _fmsMessage)
        {
            //Add schleife only if not already
            if (FmsMessages.Where(f => f.Identifier == _fmsMessage.Identifier).FirstOrDefault() != null ||
                _fmsMessage.Vehicle == null)
                return;

            FmsMessages.Add(_fmsMessage);
        }

        public void AddPager(PagerMessageEventArgs _pagerMessage)
        {
            //Add schleife only if not already
            if (PagerMessages.Where(z => z.Identifier == _pagerMessage.Identifier).FirstOrDefault() != null ||
                _pagerMessage.Pager == null)
                return;

            PagerMessages.Add(_pagerMessage);
        }
    }
}