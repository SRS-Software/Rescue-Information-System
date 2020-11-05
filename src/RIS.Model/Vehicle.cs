#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class Vehicle : EntityBase
    {
        public Vehicle()
        {
            AaoVehicles = new HashSet<AaoVehicle>();
            AlarmappGroups = new HashSet<AlarmappGroup>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string ViewText { get; set; }
        public string FaxText { get; set; }
        public string BosIdentifier { get; set; }
        public int? MainColumn { get; set; }
        public int? MainRow { get; set; }
        public string File { get; set; }
        public virtual ICollection<AaoVehicle> AaoVehicles { get; set; }
        public virtual ICollection<AlarmappGroup> AlarmappGroups { get; set; }
    }
}