using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace PlexRequests.IntegrationTests.Triggers
{
    [Collection("IntegrationTests")]
    public class HealthHttpTriggerTests : PlexRequestsIntegrationTests
    {
        [Fact]
        public async Task GetHealth_WhenCallingFunction_ShouldReturnOk()
        {
            var result  = await GetHealth();

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
