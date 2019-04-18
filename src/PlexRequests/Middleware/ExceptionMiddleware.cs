using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlexRequests.Core.Exceptions;

namespace PlexRequests.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private static async Task HandleException(HttpContext context, Exception exception)
        {
            var errorMessage = "An unexpected error has occured";
            var statusCode = HttpStatusCode.InternalServerError;
            string description = null;

            var logger = context.RequestServices.GetService<ILogger<ExceptionMiddleware>>();
            
            object debugLogObject = null;
            if (exception is PlexRequestException plexRequestException)
            {
                statusCode = plexRequestException.StatusCode;
                debugLogObject = plexRequestException.LoggableObject;
                errorMessage = plexRequestException.Message;
                description = plexRequestException.Description;
            }

            logger.LogError(exception, errorMessage);
            if (debugLogObject != null)
            {
                logger.LogDebug(JsonConvert.SerializeObject(debugLogObject));
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var response = JsonConvert.SerializeObject(new ErrorResponse(errorMessage, description));
            await context.Response.WriteAsync(response);
        }
    }
}
