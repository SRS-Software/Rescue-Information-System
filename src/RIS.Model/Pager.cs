#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class Pager : EntityBase
    {
        public Pager()
        {
            Amss = new HashSet<Ams>();
            AlarmappGroups = new HashSet<AlarmappGroup>();
        }

        [Required(ErrorMessage = "Identifier is required")]
        public string Identifier { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public bool Priority { get; set; }

        public string File { get; set; }
        public virtual ICollection<Ams> Amss { get; set; }
        public virtual ICollection<AlarmappGroup> AlarmappGroups { get; set; }
    }
}