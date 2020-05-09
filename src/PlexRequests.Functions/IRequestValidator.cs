using PlexRequests.Functions.Features;

namespace PlexRequests.Functions
{
    public interface IRequestValidator
    {
        ValidationContext<T> ValidateRequest<T>(T requestData) where T : class;
    }
}
