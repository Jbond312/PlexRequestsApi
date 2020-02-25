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

        public static ActionResult ToResultIfValid<T, TA>(this ValidationContext<T> context) where T : class where TA : ActionResult, new()
        {
            if (!context.IsSuccessful)
            {
                return new BadRequestObjectResult(CreateErrors(context.ValidationErrors));
            }

            return new TA();
        }

        public static ActionResult ToResultIfValid<T>(this ValidationContext context) where T : ActionResult, new()
        {
            if (!context.IsSuccessful)
            {
                return new BadRequestObjectResult(CreateErrors(context.ValidationErrors));
            }

            return new T();
        }

        private static List<ApiErrorResponse> CreateErrors(List<ValidationResult> validationResults)
        {
            return validationResults.Select(x => new ApiErrorResponse(x.Message, x.Description)).ToList();
        }
    }
}
