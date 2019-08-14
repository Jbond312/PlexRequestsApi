using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PlexRequests.Core.Auth;

namespace PlexRequests.Attributes
{
    public class ValidRoleAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("The Roles field is required.");
            }

            var roles = new List<string>{
                PlexRequestRoles.Admin,
                PlexRequestRoles.User,
                PlexRequestRoles.Commenter
            };

            var inputRoles = (List<string>)value;

            if (inputRoles.Count != inputRoles.Distinct().Count())
            {
                return new ValidationResult("The Roles field must contain unique roles.");
            }

            var diff = inputRoles.Where(x => !roles.Contains(x)).ToList();

            if (diff.Any())
            {
                var invalidRoles = string.Join(", ", diff);
                var errMsg = $"The Roles field contains invalid roles: {invalidRoles}";
                return new ValidationResult(errMsg);
            }

            return ValidationResult.Success;
        }
    }
}