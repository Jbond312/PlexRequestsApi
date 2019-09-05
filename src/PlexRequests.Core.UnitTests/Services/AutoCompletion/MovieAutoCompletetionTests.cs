using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Services;
using PlexRequests.Core.Services.AutoCompletion;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Services.AutoCompletion
{
    public class MovieAutoCompletetionTests
    {
        private readonly MovieAutoCompletion _underTest;

        private readonly IMovieRequestService _requestService;

        private readonly Fixture _fixture;

        private Dictionary<MediaAgent, PlexMediaItem> _agentsForPlexItems;
        private List<MovieRequest> _movieRequests;
        private Func<Task> _commandAction;

        public MovieAutoCompletetionTests()
        {
            _fixture = new Fixture();

            _requestService = Substitute.For<IMovieRequestService>();

            _underTest = new MovieAutoCompletion(_requestService);
        }

        [Fact]
        public void When_No_Matching_Requests_No_Requests_Are_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenNoMatchingRequests())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenNoRequestsWereUpdated())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Movie_Request_On_Primary_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingRequestPrimaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenOneRequestWasUpdated())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Movie_Request_On_Fallback_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingRequestSecondaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenOneRequestWasUpdated())
                .BDDfy();
        }

        private void GivenRequestsAgentsForPlexMediaItems()
        {
            _agentsForPlexItems = _fixture.CreateMany<KeyValuePair<MediaAgent, PlexMediaItem>>()
                                          .ToDictionary(x => x.Key, x => x.Value);
        }

        private void GivenNoMatchingRequests()
        {
            _movieRequests = _fixture.CreateMany<MovieRequest>().ToList();

            _requestService.GetIncompleteRequests().Returns(_movieRequests);
        }

        private void GivenASingleMatchingRequestPrimaryAgent()
        {
            var request = _fixture.Create<MovieRequest>();

            request.PrimaryAgent = GetMatchingAgent();

            _movieRequests = new List<MovieRequest> {request};

            _requestService.GetIncompleteRequests().Returns(_movieRequests);
        }

        private void GivenASingleMatchingRequestSecondaryAgent()
        {
            var request = _fixture.Create<MovieRequest>();

            var firstPlexAgent = _agentsForPlexItems.First().Key;

            var additionalAgent = new MediaAgent(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
            request.AdditionalAgents = new List<MediaAgent> {additionalAgent};

            _movieRequests = new List<MovieRequest> {request};

            _requestService.GetIncompleteRequests().Returns(_movieRequests);
        }

        private void WhenCommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.AutoComplete(_agentsForPlexItems);
        }

        private void ThenResponseIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenNoRequestsWereUpdated()
        {
            _requestService.DidNotReceive().Update(Arg.Any<MovieRequest>());
        }

        private void ThenOneRequestWasUpdated()
        {
            _requestService.Received().Update(Arg.Any<MovieRequest>());
        }

        private MediaAgent GetMatchingAgent()
        {
            var firstPlexAgent = _agentsForPlexItems.First().Key;

            return new MediaAgent(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
        }
    }
}