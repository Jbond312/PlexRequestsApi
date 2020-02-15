using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Services;
using PlexRequests.Core.Services.AutoCompletion;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Services.AutoCompletion
{
    public class TvAutoCompletionTests
    {
        private readonly TvAutoCompletion _underTest;

        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly Fixture _fixture;

        private Dictionary<MediaAgent, PlexMediaItemRow> _agentsForPlexItems;
        private List<TvRequestRow> _tvRequests;
        private Func<Task> _commandAction;
        private TvRequestRow _request;

        public TvAutoCompletionTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _requestService = Substitute.For<ITvRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _underTest = new TvAutoCompletion(_requestService, _unitOfWork);
        }

        [Fact]
        public void When_No_Matching_Tv_Requests_No_Requests_Are_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenNoMatchingTvRequests())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Tv_Request_On_Primary_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingTvRequestPrimaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Tv_Request_On_Fallback_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingTvRequestSecondaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void Calls_Aggregate_Status_For_Tv_Shows()
        {
            bool isTracked = false;
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenAMatchingTvRequestWithAllMatchingEpisodes(isTracked))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenUpdatedRequestShouldBeCorrect(RequestStatuses.Completed))
                .Then(x => x.ThenAggregateStatusIsCorrect())
                .Then(x => x.ThenChangesAreCommitted())
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
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenRequestsAgentsForPlexMediaItems()
        {
            _agentsForPlexItems = _fixture.CreateMany<KeyValuePair<MediaAgent, PlexMediaItemRow>>().ToDictionary(x => x.Key, x => x.Value);
        }


        private void GivenNoMatchingTvRequests()
        {
            _tvRequests = _fixture.Build<TvRequestRow>()
                                .With(x => x.Track, false)
                                .CreateMany()
                                .ToList();

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }

        private void GivenASingleMatchingTvRequestPrimaryAgent()
        {
            var request = _fixture.Build<TvRequestRow>()
                                  .With(x => x.Track, false)
                                  .Create();

            request.TvRequestAgents.Add(GetMatchingAgent());

            _tvRequests = new List<TvRequestRow> { request };

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }


        private void GivenASingleMatchingTvRequestSecondaryAgent()
        {
            var request = _fixture.Build<TvRequestRow>()
                                  .With(x => x.Track, false)
                                  .Create();

            var firstPlexAgent = _agentsForPlexItems.First().Key;

            var additionalAgent = new MediaAgent(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
            request.TvRequestAgents = new List<TvRequestAgentRow>
            {
                new TvRequestAgentRow(additionalAgent.AgentType, additionalAgent.AgentSourceId)
            };

            _tvRequests = new List<TvRequestRow> { request };

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }

        private void GivenAMatchingTvRequestWithAllMatchingEpisodes(bool istracked)
        {
            var plexItem = _agentsForPlexItems.First().Value;

            _request = _fixture.Create<TvRequestRow>();
            _request.TvRequestAgents.Add(GetMatchingAgent());
            _request.TvRequestSeasons = new List<TvRequestSeasonRow>();
            _request.Track = istracked;

            if (!istracked)
            {
                MirrorPlexSeasons(plexItem, _request, false, false);
            }

            _tvRequests = new List<TvRequestRow> { _request };

            _requestService.GetIncompleteRequests().Returns(_tvRequests);
        }

        private void WhenCommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.AutoComplete(_agentsForPlexItems);
        }

        private void ThenResponseIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenUpdatedRequestShouldBeCorrect(RequestStatuses expectedStatus)
        {
            _request.Should().NotBeNull();
            _request.RequestStatus.Should().Be(expectedStatus);
        }

        private void ThenAggregateStatusIsCorrect()
        {
            _request.Should().NotBeNull();
            _requestService.Received().SetAggregatedStatus(Arg.Any<TvRequestRow>());
        }

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }

        private TvRequestAgentRow GetMatchingAgent()
        {
            var firstPlexAgent = _agentsForPlexItems.First().Key;

            return new TvRequestAgentRow(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
        }

        private void MirrorPlexSeasons(PlexMediaItemRow plexItem, TvRequestRow request, bool createExtraSeason, bool createExtraEpisode)
        {
            foreach (var season in plexItem.PlexSeasons)
            {
                var requestSeason = new TvRequestSeasonRow
                {
                    SeasonIndex = season.Season,
                    TvRequestEpisodes = new List<TvRequestEpisodeRow>()
                };

                foreach (var episode in season.PlexEpisodes)
                {
                    var requestEpisode = new TvRequestEpisodeRow
                    {
                        EpisodeIndex = episode.Episode
                    };

                    requestSeason.TvRequestEpisodes.Add(requestEpisode);
                }

                request.TvRequestSeasons.Add(requestSeason);
            }

            if (createExtraSeason)
            {
                request.TvRequestSeasons.Add(_fixture.Create<TvRequestSeasonRow>());
            }

            if (createExtraEpisode)
            {
                request.TvRequestSeasons.ElementAt(0).TvRequestEpisodes.Add(_fixture.Create<TvRequestEpisodeRow>());
            }
        }
    }
}
