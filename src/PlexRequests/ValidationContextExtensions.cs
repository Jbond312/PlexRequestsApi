using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests;

namespace PlexRequests
{
    public static class ValidationContextExtensions
    {
        public static ActionResult ToOkIfValidResult<T>(this ValidationContext<T> context) where T : class
        {
            if(!context.IsSuccessful)
            {
                return new BadRequestObjectResult(CreateErrors(context.ValidationErrors));
            }

            return new OkObjectResult(context.Data);
        }

        public static ActionResult ToOkIfValidResult(this ValidationContext context)
        {
            if (!context.IsSuccessful)
            {
                return new BadRequestObjectResult(CreateErrors(context.ValidationErrors));
            }

            return new OkResult();
        }

        private static List<ApiErrorResponse> CreateErrors(List<ValidationResult> validationResults)
        {
            return validationResults.Select(x => new ApiErrorResponse(x.Message, x.Description)).ToList();
        }
    }
}
