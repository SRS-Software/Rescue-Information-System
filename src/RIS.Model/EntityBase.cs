#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

#endregion

namespace RIS.Model
{
    public class EntityBase : IDataErrorInfo
    {
        protected EntityBase()
        {
            Errors = new Dictionary<string, string>();
        }

        #region Public Properties

        [Key] public int Id { get; set; }

        #endregion //Public Properties

        #region IDataErrorInfo

        public readonly Dictionary<string, string> Errors;
        public bool IsValid => Errors.Count == 0;

        public void AddError(string propertyName, string message)
        {
            if (!Errors.ContainsKey(propertyName)) Errors[propertyName] = message;
        }

        public void RemoveError(string propertyName)
        {
            Errors.Remove(propertyName);
        }


        public string Error => string.Empty;

        public string this[string memberName]
        {
            get
            {
                //Check Property for validation error
                var validationResults = new List<ValidationResult>();
                if (string.IsNullOrEmpty(memberName))
                {
                    Validator.TryValidateObject(this, new ValidationContext(this, null, null), validationResults, true);
                }
                else
                {
                    RemoveError(memberName);
                    var property = TypeDescriptor.GetProperties(this)[memberName];
                    if (property == null)
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                            "The specified member {0} was not found on the instance {1}", memberName, GetType()));

                    Validator.TryValidateProperty(property.GetValue(this), new ValidationContext(this, null, null)
                    {
                        MemberName = memberName
                    }, validationResults);
                }

                //Create a string foreach error
                var errorBuilder = new StringBuilder();
                foreach (var validationResult in validationResults)
                    errorBuilder.AppendLine(validationResult.ErrorMessage);

                //Create result
                var _result = errorBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(_result) && memberName != null) Errors.Add(memberName, _result);

                return _result;
            }
        }

        #endregion //IDataErrorInfo
    }
}