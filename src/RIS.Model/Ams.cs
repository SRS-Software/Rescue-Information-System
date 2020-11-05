#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class Ams : EntityBase
    {
        public Ams()
        {
            Users = new HashSet<User>();
            Pagers = new HashSet<Pager>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Pager> Pagers { get; set; }
    }
}