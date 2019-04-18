using System;
using System.Net;
using FluentAssertions;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Repository.Enums;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests
{
    public class AgentGuidParserTests
    {
        private readonly IAgentGuidParser _underTest;

        private Func<(AgentTypes agentType, string agentSourceId)> _commandAction;
        
        private (AgentTypes agentType, string agentSourceId) _responseAgentDetails;

        public AgentGuidParserTests()
        {
            _underTest = new AgentGuidParser();
        }

        [Theory]
        [InlineData("com.plexapp.agents.thetvdb://73141/15/7?lang=en", AgentTypes.TheTvDb, "73141")]
        [InlineData("com.plexapp.agents.thetvdb://73141/15?lang=en", AgentTypes.TheTvDb, "73141")]
        [InlineData("com.plexapp.agents.thetvdb://73141?lang=en", AgentTypes.TheTvDb, "73141")]
        [InlineData("com.plexapp.agents.imdb://tt1727824?lang=en", AgentTypes.Imdb, "tt1727824")]
        [InlineData("com.plexapp.agents.themoviedb://446021?lang=en", AgentTypes.TheMovieDb, "446021")]
        private void Returns_Correct_Agent_Details(string agentGuid, AgentTypes expectedAgentType, string expectedAgentId)
        {
            this.When(x => x.WhenACommandActionIsCreated(agentGuid))
                .Then(x => x.ThenTheActionShouldBeSuccessful())
                .Then(x => x.TheAgentDetailsShouldBeCorrect(expectedAgentType, expectedAgentId))
                .BDDfy();
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        private void Throws_Error_When_No_AgentGuid_Specified(string agentGuid)
        {
            this.When(x => x.WhenACommandActionIsCreated(agentGuid))
                .Then(x => x.ThenAnErrorIsThrown("Invalid Plex Metadata Agent Guid", "The PlexMetadataGuid should not be null or empty", HttpStatusCode.InternalServerError, null))
                .BDDfy();
        }

        [Theory]
        [InlineData("abcd")]
        [InlineData("a.b.c.d")]
        [InlineData("a.b.c.d:1234")]
        [InlineData("a.b.c.d:/1234")]
        [InlineData("a.b.c.d://1234")]
        [InlineData("://1234")]
        [InlineData("//1234")]
        [InlineData("/1234")]
        [InlineData("com.plexapp.agents.imdb:3141")]
        [InlineData("com.plexapp.agents.imdb:/3141")]
        [InlineData("com.plexapp.agents.imdb://3141")]
        [InlineData("...themoviedb://446021?lang=en")]
        private void Throws_Error_When_Invalid_AgentGuid_Specified(string agentGuid)
        {
            this.When(x => x.WhenACommandActionIsCreated(agentGuid))
                .Then(x => x.ThenAnErrorIsThrown("Invalid Plex Metadata Agent Guid", "The PlexMetadataGuid was not in the expected format", HttpStatusCode.InternalServerError, agentGuid))
                .BDDfy();
        }
        
        [Theory]
        [InlineData("com.plexapp.agents.foo://73141/15/7?lang=en")]
        private void Throws_Error_When_Invalid_AgentType_Found(string agentGuid)
        {
            this.When(x => x.WhenACommandActionIsCreated(agentGuid))
                .Then(x => x.ThenAnErrorIsThrown("Invalid Plex Metadata Agent Guid", "No AgentType could be extracted from the agent guid", HttpStatusCode.InternalServerError, agentGuid))
                .BDDfy();
        }
        
        private void WhenACommandActionIsCreated(string agentGuid)
        {
            _commandAction = () => _underTest.TryGetAgentDetails(agentGuid);
        }

        private void ThenTheActionShouldBeSuccessful()
        {
            _responseAgentDetails = _commandAction.Should().NotThrow().Subject;
        }
                
        private void TheAgentDetailsShouldBeCorrect(AgentTypes expectedAgentType, string expectedAgentId)
        {
            _responseAgentDetails.agentType.Should().Be(expectedAgentType);
            _responseAgentDetails.agentSourceId.Should().Be(expectedAgentId);
        }

        private void ThenAnErrorIsThrown(string message, string description, HttpStatusCode statusCode, object logObject)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(message)
                          .Where(x => x.Description == description)
                          .Where(x => x.StatusCode == statusCode)
                          .Where(x => logObject != null && x.LoggableObject == logObject || logObject == null);
        }
    }
}