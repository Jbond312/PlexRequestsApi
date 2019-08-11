using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Services
{
    public class CompletionServiceTests
    {
        private readonly CompletionService _underTest;

        private readonly IRequestService _requestService;

        private readonly Fixture _fixture;

        private Dictionary<MediaAgent, PlexMediaItem> _agentsForPlexItems;
        private List<Request> _requests;
        private Func<Task> _commandAction;
        private Request _updatedRequest;
        private RequestStatuses _aggregateStatus;

        public CompletionServiceTests()
        {
            _fixture = new Fixture();

            _requestService = Substitute.For<IRequestService>();

            _underTest = new CompletionService(_requestService);
        }

        [Theory]
        [InlineData(PlexMediaTypes.Show)]
        [InlineData(PlexMediaTypes.Movie)]
        public void When_No_Matching_Requests_No_Requests_Are_Updated(PlexMediaTypes mediaType)
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenNoMatchingRequests())
                .When(x => x.WhenCommandActionIsCreated(mediaType))
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenNoRequestsWereUpdated())
                .BDDfy();
        }

        [Theory]
        [InlineData(PlexMediaTypes.Show)]
        [InlineData(PlexMediaTypes.Movie)]
        private void When_A_Matching_Request_On_Primary_Agent_Request_Is_Updated(PlexMediaTypes mediaType)
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingRequestPrimaryAgent())
                .When(x => x.WhenCommandActionIsCreated(mediaType))
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenOneRequestWasUpdated())
                .BDDfy();
        }

        [Theory]
        [InlineData(PlexMediaTypes.Show)]
        [InlineData(PlexMediaTypes.Movie)]
        private void When_A_Matching_Request_On_Fallback_Agent_Request_Is_Updated(PlexMediaTypes mediaType)
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingRequestSecondaryAgent())
                .When(x => x.WhenCommandActionIsCreated(mediaType))
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenOneRequestWasUpdated())
                .BDDfy();
        }

        [Fact]
        private void Calls_Aggregate_Status_For_Tv_Shows()
        {
            bool isTracked = false;
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenAMatchingRequestWithAllMatchingEpisodes(isTracked))
                .Given(x => x.GivenARequestIsUpdated())
                .Given(x => x.GivenAggregateStatusIsRetrieved())
                .When(x => x.WhenCommandActionIsCreated(PlexMediaTypes.Show))
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenUpdatedRequestShouldBeCorrect(RequestStatuses.Completed))
                .Then(x => x.ThenAggregateStatusIsCorrect())
                .BDDfy();
        }

        [Fact]
        private void Request_Not_Updated_For_Tracked_Tv_Show()
        {
            bool isTracked = true;
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenAMatchingRequestWithAllMatchingEpisodes(isTracked))
                .When(x => x.WhenCommandActionIsCreated(PlexMediaTypes.Show))
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenNoRequestIsUpdated())
                .BDDfy();
        }

        private void GivenRequestsAgentsForPlexMediaItems()
        {
            _agentsForPlexItems = _fixture.CreateMany<KeyValuePair<MediaAgent, PlexMediaItem>>().ToDictionary(x => x.Key, x => x.Value);
        }

        private void GivenNoMatchingRequests()
        {
            _requests = _fixture.Build<Request>()
                .With(x => x.Track, false)
                .CreateMany()
                .ToList();

            _requestService.GetIncompleteRequests(Arg.Any<PlexMediaTypes>()).Returns(_requests);
        }

        private void GivenASingleMatchingRequestPrimaryAgent()
        {
            var request = _fixture.Build<Request>()
            .With(x => x.Track, false)
            .Create();

            request.PrimaryAgent = GetMatchingAgent();

            _requests = new List<Request> { request };

            _requestService.GetIncompleteRequests(Arg.Any<PlexMediaTypes>()).Returns(_requests);
        }

        private void GivenASingleMatchingRequestSecondaryAgent()
        {
            var request = _fixture.Build<Request>()
            .With(x => x.Track, false)
            .Create();

            var firstPlexAgent = _agentsForPlexItems.First().Key;

            var additionalAgent = new MediaAgent(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
            request.AdditionalAgents = new List<MediaAgent> { additionalAgent };

            _requests = new List<Request> { request };

            _requestService.GetIncompleteRequests(Arg.Any<PlexMediaTypes>()).Returns(_requests);
        }

        private void GivenAMatchingRequestWithAllMatchingEpisodes(bool istracked)
        {
            var plexItem = _agentsForPlexItems.First().Value;

            var request = _fixture.Create<Request>();
            request.PrimaryAgent = GetMatchingAgent();
            request.Seasons = new List<RequestSeason>();
            request.Track = istracked;

            if (!istracked)
            {
                MirrorPlexSeasons(plexItem, request, false, false);
            }

            _requests = new List<Request> { request };

            _requestService.GetIncompleteRequests(Arg.Any<PlexMediaTypes>()).Returns(_requests);
        }

        private void GivenARequestIsUpdated()
        {
            _requestService.Update(Arg.Do<Request>(x => _updatedRequest = x));
        }

        private void GivenAggregateStatusIsRetrieved()
        {
            _aggregateStatus = RequestStatuses.Completed;
            _requestService.CalculateAggregatedStatus(Arg.Any<Request>()).Returns(_aggregateStatus);
        }

        private void WhenCommandActionIsCreated(PlexMediaTypes mediaType)
        {
            _commandAction = async () => await _underTest.AutoCompleteRequests(_agentsForPlexItems, mediaType);
        }

        private void ThenResponseIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenNoRequestsWereUpdated()
        {
            _requestService.DidNotReceive().Update(Arg.Any<Request>());
        }

        private void ThenOneRequestWasUpdated()
        {
            _requestService.Received().Update(Arg.Any<Request>());
        }

        private void ThenUpdatedRequestShouldBeCorrect(RequestStatuses expectedStatus)
        {
            _updatedRequest.Should().NotBeNull();
            _updatedRequest.Status.Should().Be(expectedStatus);
        }

        private void ThenAggregateStatusIsCorrect()
        {
            _updatedRequest.Should().NotBeNull();
            _updatedRequest.Status.Should().Be(_aggregateStatus);
            _requestService.Received().CalculateAggregatedStatus(Arg.Any<Request>());
        }

        private void ThenNoRequestIsUpdated()
        {
            _requestService.DidNotReceive().Update(Arg.Any<Request>());
        }

        private MediaAgent GetMatchingAgent()
        {
            var firstPlexAgent = _agentsForPlexItems.First().Key;

            return new MediaAgent(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
        }

        private void MirrorPlexSeasons(PlexMediaItem plexItem, Request request, bool createExtraSeason, bool createExtraEpisode)
        {
            foreach (var season in plexItem.Seasons)
            {
                var requestSeason = new RequestSeason
                {
                    Index = season.Season,
                    Episodes = new List<RequestEpisode>()
                };

                foreach (var episode in season.Episodes)
                {
                    var requestEpisode = new RequestEpisode
                    {
                        Index = episode.Episode
                    };

                    requestSeason.Episodes.Add(requestEpisode);
                }

                request.Seasons.Add(requestSeason);
            }

            if (createExtraSeason)
            {
                request.Seasons.Add(_fixture.Create<RequestSeason>());
            }

            if (createExtraEpisode)
            {
                request.Seasons[0].Episodes.Add(_fixture.Create<RequestEpisode>());
            }
        }
    }
}