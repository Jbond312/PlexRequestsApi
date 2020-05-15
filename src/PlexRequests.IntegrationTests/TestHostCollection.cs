using Xunit;

namespace PlexRequests.IntegrationTests
{
    [CollectionDefinition("IntegrationTests")]
    public class TestHostCollection : ICollectionFixture<TestHostFixture>
    {
    }
}
