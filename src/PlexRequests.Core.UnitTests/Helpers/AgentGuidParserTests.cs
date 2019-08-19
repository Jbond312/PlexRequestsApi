using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using PlexRequests.Core.Helpers;
using PlexRequests.Repository.Enums;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Helpers
{
    public class AgentGuidParserTests
    {
        private readonly IAgentGuidParser _underTest;

        private Func<AgentGuidParserResult> _commandAction;

        private AgentGuidParserResult _responseAgentDetails;

        public AgentGuidParserTests()
        {
            var logger = NSubstitute.Substitute.For<ILogger<AgentGuidParser>>();
            _underTest = new AgentGuidParser(logger);
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
        private void Returns_Null_When_No_AgentGuid_Specified(string agentGuid)
        {
            this.When(x => x.WhenACommandActionIsCreated(agentGuid))
                .Then(x => x.ThenTheActionShouldBeSuccessful())
                .Then(x => x.ThenNullResult())
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
        private void Returns_Null_When_Invalid_AgentGuid_Specified(string agentGuid)
        {
            this.When(x => x.WhenACommandActionIsCreated(agentGuid))
                .Then(x => x.ThenTheActionShouldBeSuccessful())
                .Then(x => x.ThenNullResult())
                .BDDfy();
        }

        [Theory]
        [InlineData("com.plexapp.agents.foo://73141/15/7?lang=en")]
        private void Returns_Null_When_Invalid_AgentType_Found(string agentGuid)
        {
            this.When(x => x.WhenACommandActionIsCreated(agentGuid))
                .Then(x => x.ThenTheActionShouldBeSuccessful())
                .Then(x => x.ThenNullResult())
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
            _responseAgentDetails.AgentType.Should().Be(expectedAgentType);
            _responseAgentDetails.AgentSourceId.Should().Be(expectedAgentId);
        }

        private void ThenNullResult()
        {
            _responseAgentDetails.Should().BeNull();
        }
    }
}