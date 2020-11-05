#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class FilterField : EntityBase
    {
        public FilterField()
        {
            Filters = new HashSet<Filter>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public virtual ICollection<Filter> Filters { get; set; }
    }
}