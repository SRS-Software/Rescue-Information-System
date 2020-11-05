#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class AaoVehicle : EntityBase
    {
        [Required(ErrorMessage = "Position is required")]
        public int Position { get; set; }

        [Required(ErrorMessage = "Aao is required")]
        public virtual Aao Aao { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public virtual Vehicle Vehicle { get; set; }
    }
}