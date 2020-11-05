#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class AaoCondition : EntityBase
    {
        public AaoCondition()
        {
            Aaos = new HashSet<Aao>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public virtual ICollection<Aao> Aaos { get; set; }
    }
}