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
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Services.AutoCompletion
{
    public class MovieAutoCompletetionTests
    {
        private readonly MovieAutoCompletion _underTest;

        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly Fixture _fixture;

        private Dictionary<MediaAgent, PlexMediaItemRow> _agentsForPlexItems;
        private List<MovieRequestRow> _movieRequests;
        private Func<Task> _commandAction;

        public MovieAutoCompletetionTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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
            _agentsForPlexItems = _fixture.CreateMany<KeyValuePair<MediaAgent, PlexMediaItemRow>>()
                                          .ToDictionary(x => x.Key, x => x.Value);
        }

        private void GivenNoMatchingRequests()
        {
            _movieRequests = _fixture.CreateMany<MovieRequestRow>().ToList();

            _requestService.GetIncompleteRequests().Returns(_movieRequests);
        }

        private void GivenASingleMatchingRequestPrimaryAgent()
        {
            var request = _fixture.Create<MovieRequestRow>();

            request.MovieRequestAgents.Add(GetMatchingAgent());

            _movieRequests = new List<MovieRequestRow> {request};

            _requestService.GetIncompleteRequests().Returns(_movieRequests);
        }

        private void GivenASingleMatchingRequestSecondaryAgent()
        {
            var request = _fixture.Create<MovieRequestRow>();

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