#region

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Core
{
    public enum DecoderMode
    {
        [Display(Name = "OFF")] [Description("")]
        OFF,

        [Display(Name = "FMS32")] [Description("")]
        FMS32,

        [Display(Name = "MONITORD")] [Description("")]
        MONITORD,

        [Display(Name = "SANDAN")] [Description("")]
        SANDAN,

        [Display(Name = "OPERATOR2")] [Description("")]
        OPERATOR2,

        [Display(Name = "SYSLOG[TCP-Server]")] [Description("")]
        SYSLOG,

        [Display(Name = "LARDIS")] [Description("")]
        LARDIS,

        [Display(Name = "TETRAcontrol")] [Description("")]
        TETRACONTROL
    }
}