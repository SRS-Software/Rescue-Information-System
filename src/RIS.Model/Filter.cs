#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace RIS.Model
{
    public class Filter : EntityBase
    {
        public Filter()
        {
            ReplaceExpression = string.Empty;
        }

        [Required(ErrorMessage = "SearchExpression is required", AllowEmptyStrings = true)]
        public string SearchExpression { get; set; }

        public string ReplaceExpression { get; set; }
        public bool DoBeforeShow { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public virtual FilterField Field { get; set; }
    }
}