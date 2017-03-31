using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace XbimPlugin.MvdXML
{
    public class RequiredValidationRule : ValidationRule
    {

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            if (value == null || object.ReferenceEquals(value, DBNull.Value))
            {
                result = new ValidationResult(false, "Required!");
            }
            return result;
        }
    }
}
