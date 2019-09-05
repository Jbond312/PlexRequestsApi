using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Services;
using PlexRequests.Core.Services.AutoCompletion;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Services.AutoCompletion
{
    public class TvAutoCompletionTests
    {
        private readonly TvAutoCompletion _underTest;

        private readonly ITvRequestService _requestService;

        private readonly Fixture _fixture;

        private Dictionary<MediaAgent, PlexMediaItem> _agentsForPlexItems;
        private List<TvRequest> _tvRequests;
        private Func<Task> _commandAction;
        private TvRequest _updatedRequest;
        private RequestStatuses _aggregateStatus;

        public TvAutoCompletionTests()
        {
            _fixture = new Fixture();

            _requestService = Substitute.For<ITvRequestService>();

            _underTest = new TvAutoCompletion(_requestService);
        }

        [Fact]
        public void When_No_Matching_Tv_Requests_No_Requests_Are_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenNoMatchingTvRequests())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenNoTvRequestsWereUpdated())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Tv_Request_On_Primary_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingTvRequestPrimaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenOneTvRequestWasUpdated())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Tv_Request_On_Fallback_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingTvRequestSecondaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenOneTvRequestWasUpdated())
                .BDDfy();
        }

        [Fact]
        private void Calls_Aggregate_Status_For_Tv_Shows()
        {
            bool isTracked = false;
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenAMatchingTvRequestWithAllMatchingEpisodes(isTracked))
                .Given(x => x.GivenATvRequestIsUpdated())
                .Given(x => x.GivenAggregateStatusIsRetrieved())
                .When(x => x.WhenCommandActionIsCreated())
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
                .Given(x => x.GivenAMatchingTvRequestWithAllMatchingEpisodes(isTracked))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenNoTvRequestIsUpdated())
                .BDDfy();
        }

        private void GivenRequestsAgentsForPlexMediaItems()
        {
            _agentsForPlexItems = _fixture.CreateMany<KeyValuePair<MediaAgent, PlexMediaItem>>().ToDictionary(x => x.Key, x => x.Value);
        }


        private void GivenNoMatchingTvRequests()
        {
            _tvRequests = _fixture.Build<TvRequest>()
                                .With(x => x.Track, false)
                                .CreateMany()
                                .ToList();

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }

        private void GivenASingleMatchingTvRequestPrimaryAgent()
        {
            var request = _fixture.Build<TvRequest>()
                                  .With(x => x.Track, false)
                                  .Create();

            request.PrimaryAgent = GetMatchingAgent();

            _tvRequests = new List<TvRequest> { request };

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }


        private void GivenASingleMatchingTvRequestSecondaryAgent()
        {
            var request = _fixture.Build<TvRequest>()
                                  .With(x => x.Track, false)
                                  .Create();

            var firstPlexAgent = _agentsForPlexItems.First().Key;

            var additionalAgent = new MediaAgent(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
            request.AdditionalAgents = new List<MediaAgent> { additionalAgent };

            _tvRequests = new List<TvRequest> { request };

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }

        private void GivenAMatchingTvRequestWithAllMatchingEpisodes(bool istracked)
        {
            var plexItem = _agentsForPlexItems.First().Value;

            var request = _fixture.Create<TvRequest>();
            request.PrimaryAgent = GetMatchingAgent();
            request.Seasons = new List<RequestSeason>();
            request.Track = istracked;

            if (!istracked)
            {
                MirrorPlexSeasons(plexItem, request, false, false);
            }

            _tvRequests = new List<TvRequest> { request };

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }

        private void GivenATvRequestIsUpdated()
        {
            _requestService.Update(Arg.Do<TvRequest>(x => _updatedRequest = x));
        }

        private void GivenAggregateStatusIsRetrieved()
        {
            _aggregateStatus = RequestStatuses.Completed;
            _requestService.CalculateAggregatedStatus(Arg.Any<TvRequest>()).Returns(_aggregateStatus);
        }

        private void WhenCommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.AutoComplete(_agentsForPlexItems);
        }

        private void ThenResponseIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenNoTvRequestsWereUpdated()
        {
            _requestService.DidNotReceive().Update(Arg.Any<TvRequest>());
        }

        private void ThenOneTvRequestWasUpdated()
        {
            _requestService.Received().Update(Arg.Any<TvRequest>());
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
            _requestService.Received().CalculateAggregatedStatus(Arg.Any<TvRequest>());
        }

        private void ThenNoTvRequestIsUpdated()
        {
            _requestService.DidNotReceive().Update(Arg.Any<TvRequest>());
        }

        private MediaAgent GetMatchingAgent()
        {
            var firstPlexAgent = _agentsForPlexItems.First().Key;

            return new MediaAgent(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
        }

        private void MirrorPlexSeasons(PlexMediaItem plexItem, TvRequest request, bool createExtraSeason, bool createExtraEpisode)
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
