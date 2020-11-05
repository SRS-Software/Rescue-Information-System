#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class FileprintCondition : EntityBase
    {
        public FileprintCondition()
        {
            Fileprints = new HashSet<Fileprint>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public virtual ICollection<Fileprint> Fileprints { get; set; }
    }
}