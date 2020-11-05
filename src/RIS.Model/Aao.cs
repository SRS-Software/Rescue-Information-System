#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class Aao : EntityBase
    {
        public Aao()
        {
            Aaos = new HashSet<Aao>();
            Vehicles = new HashSet<AaoVehicle>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Expression is required")]
        public string Expression { get; set; }

        [Required] public virtual AaoCondition Condition { get; set; }

        public virtual Aao Combination { get; set; }
        public virtual ICollection<Aao> Aaos { get; set; }
        public virtual ICollection<AaoVehicle> Vehicles { get; set; }
    }
}