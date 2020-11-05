#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class AlarmappGroup : EntityBase
    {
        public AlarmappGroup()
        {
            Vehicles = new HashSet<Vehicle>();
            Pagers = new HashSet<Pager>();
        }

        [Required(ErrorMessage = "GroupId is required")]
        public string GroupId { get; set; }

        [Required(ErrorMessage = "GroupName is required")]
        public string GroupName { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public AlarmappDepartment Department { get; set; }

        public bool FaxOn { get; set; }
        public bool OnlyWithPager { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
        public virtual ICollection<Pager> Pagers { get; set; }
    }
}