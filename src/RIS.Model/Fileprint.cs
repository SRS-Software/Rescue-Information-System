#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class Fileprint : EntityBase
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        public virtual FileprintCondition Condition { get; set; }

        [Required(ErrorMessage = "Expression is required")]
        public string Expression { get; set; }

        [Required(ErrorMessage = "File is required")]
        public string File { get; set; }

        public virtual Printer Printer { get; set; }
    }
}