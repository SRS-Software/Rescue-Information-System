#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class User : EntityBase
    {
        public User()
        {
            Amss = new HashSet<Ams>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "MailAdresse is required")]
        public string MailAdresse { get; set; }

        public bool FaxMessageService_MailOn { get; set; }
        public bool FaxMessageService_FaxOn { get; set; }
        public bool AlarmMessageService_RecordOn { get; set; }
        public virtual ICollection<Ams> Amss { get; set; }
    }
}