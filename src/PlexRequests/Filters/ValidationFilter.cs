using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PlexRequests.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var modelState = context.ModelState.Values.First(x => x.ValidationState == ModelValidationState.Invalid);
                var errorMessage = string.Join(" ", modelState.Errors.Select(e => e.ErrorMessage));
                var response = new ApiErrorResponse("Invalid Request", errorMessage);
                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}