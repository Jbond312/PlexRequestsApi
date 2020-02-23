namespace PlexRequests.UnitTests.Builders
{
    public interface IBuilder<out T> where T : class
    {
        T Build();
    }
}
