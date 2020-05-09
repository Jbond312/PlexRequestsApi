using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PlexRequests.Functions.Features;
using DataAnnotationValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using DataAnnotationValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace PlexRequests.Functions
{
    public class RequestValidator : IRequestValidator
    {
        public ValidationContext<T> ValidateRequest<T>(T requestData) where T : class
        {
            var validationContext = new ValidationContext<T>
            {
                Data = requestData
            };
            var requestValidationResults = new List<DataAnnotationValidationResult>();
            Validator.TryValidateObject(validationContext.Data, new DataAnnotationValidationContext(validationContext.Data, null, null), requestValidationResults, true);
            validationContext.ValidationErrors = requestValidationResults.Select(x => new Features.ValidationResult($"Invalid data in request. Properties [{string.Join(", ", x.MemberNames)}]" ,x.ErrorMessage)).ToList();
            return validationContext;
        }
    }
}
