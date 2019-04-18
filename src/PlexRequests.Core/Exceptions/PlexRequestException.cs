using System;
using System.Net;

namespace PlexRequests.Core.Exceptions
{
    public class PlexRequestException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string Description { get; }
        public object LoggableObject { get; }

        public PlexRequestException(string message, string description = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object loggableObject = null) : base(message)
        {
            Description = description;
            StatusCode = statusCode;
            LoggableObject = loggableObject;
        }
    }
}
