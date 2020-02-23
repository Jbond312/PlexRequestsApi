using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Services;
using PlexRequests.Core.Services.AutoCompletion;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.UnitTests.Builders;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Services.AutoCompletion
{
    public class MovieAutoCompletetionTests
    {
        private readonly MovieAutoCompletion _underTest;

        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly Dictionary<MediaAgent, PlexMediaItemRow> _agentsForPlexItems = new Dictionary<MediaAgent, PlexMediaItemRow>();
        private List<MovieRequestRow> _movieRequests;
        private Func<Task> _commandAction;

        public MovieAutoCompletetionTests()
        {
            _requestService = Substitute.For<IMovieRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _underTest = new MovieAutoCompletion(_requestService, _unitOfWork);
        }

        [Fact]
        public void When_No_Matching_Requests_No_Requests_Are_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenNoMatchingRequests())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Movie_Request_On_Primary_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingRequestPrimaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void When_A_Matching_Movie_Request_On_Fallback_Agent_Request_Is_Updated()
        {
            this.Given(x => x.GivenRequestsAgentsForPlexMediaItems())
                .Given(x => x.GivenASingleMatchingRequestSecondaryAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenRequestsAgentsForPlexMediaItems()
        {
            var mediaAgents = new List<MediaAgent>
            {
                new MediaAgentBuilder().WithAgentType(AgentTypes.Imdb).Build(),
                new MediaAgentBuilder().WithAgentType(AgentTypes.TheTvDb).Build(),
                new MediaAgentBuilder().WithAgentType(AgentTypes.TheMovieDb).Build()
            };

            var plexMediaItemRows = new MoviePlexMediaItemRowBuilder().CreateMany();

            for (var i = 0; i < mediaAgents.Count; i++)
            {
                _agentsForPlexItems.Add(mediaAgents[i], plexMediaItemRows[i]);
            }
        }

        private void GivenNoMatchingRequests()
        {
            _movieRequests = new MovieRequestRowBuilder().CreateMany();

            _requestService.GetIncompleteRequests().Returns(_movieRequests);
        }

        private void GivenASingleMatchingRequestPrimaryAgent()
        {
            var request = new MovieRequestRowBuilder().Build();

            request.MovieRequestAgents.Add(GetMatchingAgent());

            _movieRequests = new List<MovieRequestRow> {request};

            _requestService.GetIncompleteRequests().Returns(_movieRequests);
        }

        private void GivenASingleMatchingRequestSecondaryAgent()
        {
            var request = new MovieRequestRowBuilder().Build();

            var firstPlexAgent = _agentsForPlexItems.First().Key;

            var additionalAgent = new MovieRequestAgentRow(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
            request.MovieRequestAgents = new List<MovieRequestAgentRow> {additionalAgent};

            _movieRequests = new List<MovieRequestRow> {request};

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

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }

        private MovieRequestAgentRow GetMatchingAgent()
        {
            var firstPlexAgent = _agentsForPlexItems.First().Key;

            return new MovieRequestAgentRow(firstPlexAgent.AgentType, firstPlexAgent.AgentSourceId);
        }
    }
}