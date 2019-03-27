namespace PlexRequests.Helpers
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string Description { get; set; }

        public ErrorResponse(string message, string description = null)
        {
            Message = message;
            Description = description;
        }
    }
}
