using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models.Requests;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class CreateTvRequestCommandHandlerTests
    {
        private readonly IRequestHandler<CreateTvRequestCommand> _underTest;

        private readonly IRequestService _requestService;
        private readonly ITheMovieDbApi _theMovieDbApi;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private readonly Fixture _fixture;

        private Func<Task> _commandAction;
        private CreateTvRequestCommand _command;
        private List<Request> _requests;
        private PlexMediaItem _plexMediaItem;
        private Request _createdRequest;
        private string _claimsUsername;
        private Guid _claimsUserId;

        public CreateTvRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<IRequestService>();
            _theMovieDbApi = Substitute.For<ITheMovieDbApi>();
            _plexService = Substitute.For<IPlexService>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            
            _underTest = new CreateTvRequestCommandHandler(_requestService, _theMovieDbApi, _plexService, _claimsPrincipalAccessor);
            
            _fixture = new Fixture();
        }

        [Fact]
        private void Throws_Error_If_All_Episodes_Already_Requested()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAllEpisodesAlreadyRequested())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created", "All TV Episodes have already been requested.", HttpStatusCode.BadRequest))
                .BDDfy();
        }
        
        [Fact]
        private void Throws_Error_When_All_Episodes_In_Request_Already_Exist_In_Plex_With_Primary_Agent_TheMovieDb()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoMatchingRequests())
                .Given(x => x.GivenAllEpisodesAlreadyInPlex())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created",
                    "All TV Episodes are already available in Plex.", HttpStatusCode.BadRequest))
                .BDDfy();
        }
        
        [Fact]
        private void Throws_Error_When_All_Episodes_In_Request_Already_Exist_In_Plex_With_Fallback_Agent_TheTvDb()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoMatchingRequests())
                .Given(x => x.GivenAllEpisodesAlreadyInPlexFromFallbackAgent())
                .Given(x => x.GivenTheTvDbExternalIdReturned())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created",
                    "All TV Episodes are already available in Plex.", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Creates_Request_Successfully_When_All_Episodes_Are_Valid()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoMatchingRequests())
                .Given(x => x.GivenNoMatchingPlexContent())
                .Given(x => x.GivenUserDetailsFromClaims())
                .Given(x => x.GivenARequestIsCreated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated(_command.Seasons.Count))
                .BDDfy();
        }

        [Fact]
        private void Creates_Request_Successfully_When_New_Episodes_But_One_Season_Already_Requested()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenOneSeasonNotAlreadyRequested())
                .Given(x => x.GivenNoMatchingPlexContent())
                .Given(x => x.GivenUserDetailsFromClaims())
                .Given(x => x.GivenARequestIsCreated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated(1))
                .BDDfy();
        }
        
        [Fact]
        private void Creates_Request_Successfully_When_New_Episodes_But_One_Season_Already_In_Plex()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoMatchingRequests())
                .Given(x => x.GivenOneSeasonNotMatchingPlexContent())
                .Given(x => x.GivenUserDetailsFromClaims())
                .Given(x => x.GivenARequestIsCreated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated(1))
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<CreateTvRequestCommand>();
        }

        private void GivenAllEpisodesAlreadyRequested()
        {
            CreateRequestsFromCommand();

            _requestService.GetExistingTvRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }
        
        private void GivenOneSeasonNotAlreadyRequested()
        {
            CreateRequestsFromCommand();

            _requests.First().Seasons.RemoveAt(0);
            
            _requestService.GetExistingTvRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }

        private void CreateRequestsFromCommand()
        {
            var request = _fixture.Create<Request>();

            request.Seasons = new List<RequestSeason>();
            foreach (var season in _command.Seasons)
            {
                var requestEpisodes = season.Episodes.Select(episode => new RequestEpisode
                {
                    Episode = episode.Episode
                });

                var requestSeason = new RequestSeason
                {
                    Season = season.Season,
                    Episodes = requestEpisodes.ToList()
                };

                request.Seasons.Add(requestSeason);
            }

            _requests = new List<Request> {request};
        }

        private void GivenAllEpisodesAlreadyInPlex()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItem>();

            _plexMediaItem.Seasons = CreatePlexSeasonsFromCommand();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }
        
        private void GivenAllEpisodesAlreadyInPlexFromFallbackAgent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItem>();

            _plexMediaItem.Seasons = CreatePlexSeasonsFromCommand();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(null, _plexMediaItem);
        }
        
        private void GivenOneSeasonNotMatchingPlexContent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItem>();

            _plexMediaItem.Seasons = CreatePlexSeasonsFromCommand();
            _plexMediaItem.Seasons.RemoveAt(0);
            
            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }

        private void GivenNoMatchingRequests()
        {
            _requests = _fixture.CreateMany<Request>().ToList();

            _requestService.GetExistingTvRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }
        
        private void GivenNoMatchingPlexContent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItem>();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }

        private void GivenTheTvDbExternalIdReturned()
        {
            _theMovieDbApi.GetTvExternalIds(Arg.Any<int>()).Returns(_fixture.Create<ExternalIds>());
        }

        private void GivenARequestIsCreated()
        {
            _createdRequest = null;
            _requestService.Create(Arg.Do<Request>(x => _createdRequest = x));
        }

        private void GivenUserDetailsFromClaims()
        {
            _claimsUsername = _fixture.Create<string>();
            _claimsUserId = _fixture.Create<Guid>();
            _claimsPrincipalAccessor.Username.Returns(_claimsUsername);
            _claimsPrincipalAccessor.UserId.Returns(_claimsUserId);
        }

        private void WhenCommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }
        
        private void ThenErrorIsThrown(string message, string description, HttpStatusCode statusCode)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(message)
                          .Where(x => x.Description == description)
                          .Where(x => x.StatusCode == statusCode);
        }

        private void ThenRequestIsCreated(int expectedSeasonCount)
        {
            _commandAction.Should().NotThrow();
            _requestService.Received().Create(Arg.Any<Request>());
            
            _createdRequest.Should().NotBeNull();
            _createdRequest.Id.Should().Be(Guid.Empty);
            _createdRequest.MediaType.Should().Be(PlexMediaTypes.Show);
            _createdRequest.IsApproved.Should().BeFalse();
            _createdRequest.AgentType.Should().Be(AgentTypes.TheMovieDb);
            _createdRequest.AgentSourceId.Should().Be(_command.TheMovieDbId.ToString());
            _createdRequest.PlexRatingKey.Should().BeNull();
            _createdRequest.Seasons.Should().NotBeNull();
            _createdRequest.Seasons.Count.Should().Be(expectedSeasonCount);
            _createdRequest.RequestedByUserName.Should().Be(_claimsUsername);
            _createdRequest.RequestedByUserId.Should().Be(_claimsUserId);

            for (var i = 0; i < expectedSeasonCount; i++)
            {
                _createdRequest.Seasons[i].Should().BeEquivalentTo(_command.Seasons[i]);
            }
        }

        private List<PlexSeason> CreatePlexSeasonsFromCommand()
        {
            var plexSeasons = new List<PlexSeason>();

            foreach (var season in _command.Seasons)
            {
                var plexEpisodes = season.Episodes.Select(episode => new PlexEpisode
                {
                    Episode = episode.Episode
                }).ToList();
                
                plexSeasons.Add(new PlexSeason
                {
                    Season = season.Season,
                    Episodes = plexEpisodes
                });
            }

            return plexSeasons.ToList();
        }
    }
}