namespace PlexRequests
{
    public class ApiErrorResponse
    {
        public ApiErrorResponse(string error, string message)
        {
            Error = error;
            Message = message;
        }

        public string Error { get; set; }
        public string Message { get; set; }
    }
}