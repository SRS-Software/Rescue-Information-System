#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class AlarmappDepartment : EntityBase
    {
        public AlarmappDepartment()
        {
            Groups = new HashSet<AlarmappGroup>();
        }

        [Required(ErrorMessage = "DepartmentId is required")]
        public string DepartmentId { get; set; }

        [Required(ErrorMessage = "DepartmentName is required")]
        public string DepartmentName { get; set; }

        public virtual ICollection<AlarmappGroup> Groups { get; set; }
    }
}