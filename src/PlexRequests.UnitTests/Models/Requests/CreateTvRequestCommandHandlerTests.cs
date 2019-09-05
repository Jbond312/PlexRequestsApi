using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.ApiRequests.Requests.Models.Create;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Mapping;
using PlexRequests.Plex;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class CreateTvRequestCommandHandlerTests
    {
        private readonly IRequestHandler<CreateTvRequestCommand> _underTest;

        private readonly ITvRequestService _requestService;
        private readonly ITheMovieDbApi _theMovieDbApi;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private readonly Fixture _fixture;

        private Func<Task> _commandAction;
        private CreateTvRequestCommand _command;
        private List<TvRequest> _requests;
        private PlexMediaItem _plexMediaItem;
        private TvRequest _createdRequest;
        private string _claimsUsername;
        private Guid _claimsUserId;
        private TvDetails _tvDetails;
        private TvSeasonDetails _season;
        private ExternalIds _externalIds;

        public CreateTvRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<ITvRequestService>();
            _theMovieDbApi = Substitute.For<ITheMovieDbApi>();
            _plexService = Substitute.For<IPlexService>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();

            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new RequestProfile()); });
            var mapper = mapperConfig.CreateMapper();

            _underTest = new CreateTvRequestCommandHandler(mapper, _requestService, _theMovieDbApi, _plexService, _claimsPrincipalAccessor);

            _fixture = new Fixture();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        private void Throws_Error_If_No_Seasons_Requested(bool hasNullSeasons)
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoSeasons(hasNullSeasons))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created", "At least one season must be given in a request.", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        private void Throws_Error_If_All_Seasons_With_No_Episodes_Requested(bool hasNullEpisodes)
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenSeasonsWithNoEpisode(hasNullEpisodes))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created", "Each requested season must have at least one episode.", HttpStatusCode.BadRequest))
                .BDDfy();
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
        private void Throws_Error_When_Duplicate_Seasons_In_Request()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenDuplicateSeasonsInCommand())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created",
                    "All seasons in a request must be unique.", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Duplicate_Episodes_In_Request()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenDuplicateEpisodesInCommand())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created",
                    "All episodes in a season must be unique.", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Thorws_Error_When_TrackShow_But_Show_Not_In_Production()
        {
            bool inProduction = false;
            this.Given(x => x.GivenACommand())
            .Given(x => x.GivenCommandTracksShow())
            .Given(x => x.GivenTheTvDetailsReturnedFromTheMovieDb())
            .Given(x => x.GivenTvDetailsInProduction(inProduction))
            .When(x => x.WhenCommandActionIsCreated())
            .Then(x => x.ThenErrorIsThrown("Request not created", "Cannot track a TV Show that is no longer in production", HttpStatusCode.BadRequest))
            .BDDfy();
        }

        [Fact]
        private void Thorws_Error_When_TrackShow_But_Show_Already_Tracked()
        {
            bool inProduction = true;
            this.Given(x => x.GivenACommand())
            .Given(x => x.GivenCommandTracksShow())
            .Given(x => x.GivenTheTvDetailsReturnedFromTheMovieDb())
            .Given(x => x.GivenTvDetailsInProduction(inProduction))
            .Given(x => x.GivenShowAlreadyBeingTracked())
            .When(x => x.WhenCommandActionIsCreated())
            .Then(x => x.ThenErrorIsThrown("Request not created", "TV Show is already being tracked", HttpStatusCode.BadRequest))
            .BDDfy();
        }

        [Fact]
        private void Creates_Request_Successfully_When_All_Episodes_Are_Valid()
        {
            const int expectedSeasonCount = 3;

            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenTheTvDetailsReturnedFromTheMovieDb())
                .Given(x => x.GivenSeasonDetailsReturnedFromTheMovieDb())
                .Given(x => x.GivenTheTvDbExternalIdReturned())
                .Given(x => x.GivenNoMatchingRequests())
                .Given(x => x.GivenNoMatchingPlexContent())
                .Given(x => x.GivenUserDetailsFromClaims())
                .Given(x => x.GivenARequestIsCreated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated(_command.Seasons.Count))
                .Then(x => x.ThenTvSeasonDetailsWereRetrieved(expectedSeasonCount))
                .BDDfy();
        }

        [Fact]
        private void Creates_Request_Successfully_When_New_Episodes_But_All_But_One_Season_Already_Requested()
        {
            const int expectedSeasonCount = 1;

            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenTheTvDetailsReturnedFromTheMovieDb())
                .Given(x => x.GivenSeasonDetailsReturnedFromTheMovieDb())
                .Given(x => x.GivenTheTvDbExternalIdReturned())
                .Given(x => x.GivenOneSeasonNotAlreadyRequested())
                .Given(x => x.GivenNoMatchingPlexContent())
                .Given(x => x.GivenUserDetailsFromClaims())
                .Given(x => x.GivenARequestIsCreated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated(expectedSeasonCount))
                .BDDfy();
        }

        [Fact]
        private void Creates_Request_Successfully_When_New_Episodes_But_All_But_One_Season_Already_In_Plex()
        {
            const int expectedSeasonCount = 1;

            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenTheTvDetailsReturnedFromTheMovieDb())
                .Given(x => x.GivenSeasonDetailsReturnedFromTheMovieDb())
                .Given(x => x.GivenTheTvDbExternalIdReturned())
                .Given(x => x.GivenNoMatchingRequests())
                .Given(x => x.GivenOneSeasonNotMatchingPlexContent())
                .Given(x => x.GivenUserDetailsFromClaims())
                .Given(x => x.GivenARequestIsCreated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated(expectedSeasonCount))
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Build<CreateTvRequestCommand>()
                .With(x => x.TrackShow, false)
                .Create();
        }

        private void GivenCommandTracksShow()
        {
            _command.TrackShow = true;
            _command.Seasons = null;
        }

        private void GivenDuplicateSeasonsInCommand()
        {
            var commandSeason = _command.Seasons[0];
            var duplicateSeason = new TvRequestSeasonCreateModel
            {
                Index = commandSeason.Index
            };
            _command.Seasons.Add(duplicateSeason);
        }

        private void GivenDuplicateEpisodesInCommand()
        {
            var commandSeason = _command.Seasons[0];
            var duplicateEpisode = new TvRequestEpisodeCreateModel
            {
                Index = commandSeason.Episodes[0].Index
            };
            commandSeason.Episodes.Add(duplicateEpisode);
        }

        private void GivenNoSeasons(bool hasNullSeasons)
        {
            _command.Seasons = hasNullSeasons ? null : new List<TvRequestSeasonCreateModel>();
        }

        private void GivenSeasonsWithNoEpisode(bool hasNullEpisodes)
        {
            foreach (var season in _command.Seasons)
            {
                season.Episodes = hasNullEpisodes ? null : new List<TvRequestEpisodeCreateModel>();
            }
        }

        private void GivenAllEpisodesAlreadyRequested()
        {
            CreateRequestsFromCommand();

            _requestService.GetExistingRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }

        private void GivenOneSeasonNotAlreadyRequested()
        {
            CreateRequestsFromCommand();

            _requests.First().Seasons.RemoveAt(0);

            _requestService.GetExistingRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }

        private void GivenShowAlreadyBeingTracked()
        {
            var existingRequest = _fixture.Build<TvRequest>()
            .With(x => x.Track, true)
            .Create();

            _requestService.GetExistingRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(new List<TvRequest> { existingRequest });
        }

        private void CreateRequestsFromCommand()
        {
            var request = _fixture.Create<TvRequest>();

            request.Seasons = new List<RequestSeason>();
            foreach (var season in _command.Seasons)
            {
                var requestEpisodes = season.Episodes.Select(episode => new RequestEpisode
                {
                    Index = episode.Index
                });

                var requestSeason = new RequestSeason
                {
                    Index = season.Index,
                    Episodes = requestEpisodes.ToList()
                };

                request.Seasons.Add(requestSeason);
            }

            _requests = new List<TvRequest> { request };
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
            _requests = _fixture.CreateMany<TvRequest>().ToList();

            _requestService.GetExistingRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
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
            _externalIds = _fixture.Create<ExternalIds>();

            _theMovieDbApi.GetTvExternalIds(Arg.Any<int>()).Returns(_externalIds);
        }

        private void GivenTheTvDetailsReturnedFromTheMovieDb()
        {
            _tvDetails = _fixture.Build<TvDetails>()
                                 .With(x => x.First_Air_Date, "2019-12-25")
                                 .Create();

            _theMovieDbApi.GetTvDetails(_command.TheMovieDbId).Returns(_tvDetails);
        }

        private void GivenTvDetailsInProduction(bool inProduction)
        {
            _tvDetails.In_Production = inProduction;
        }

        private void GivenSeasonDetailsReturnedFromTheMovieDb()
        {
            _season = _fixture.Create<TvSeasonDetails>();

            _theMovieDbApi.GetTvSeasonDetails(Arg.Any<int>(), Arg.Any<int>()).Returns(_season);

        }

        private void GivenARequestIsCreated()
        {
            _createdRequest = null;
            _requestService.Create(Arg.Do<TvRequest>(x => _createdRequest = x));
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
            _requestService.Received().Create(Arg.Any<TvRequest>());

            _createdRequest.Should().NotBeNull();
            _createdRequest.Id.Should().Be(Guid.Empty);
            _createdRequest.Status.Should().Be(RequestStatuses.PendingApproval);
            _createdRequest.PrimaryAgent.AgentType.Should().Be(AgentTypes.TheMovieDb);
            _createdRequest.PrimaryAgent.AgentSourceId.Should().Be(_command.TheMovieDbId.ToString());
            _createdRequest.PlexMediaUri.Should().BeNull();
            _createdRequest.Seasons.Should().NotBeNull();
            _createdRequest.Seasons.Count.Should().Be(expectedSeasonCount);
            _createdRequest.RequestedByUserName.Should().Be(_claimsUsername);
            _createdRequest.RequestedByUserId.Should().Be(_claimsUserId);
            _createdRequest.Title.Should().Be(_tvDetails.Name);
            _createdRequest.AirDate.Should().Be(DateTime.Parse(_tvDetails.First_Air_Date));
            _createdRequest.ImagePath.Should().Be(_tvDetails.Poster_Path);
            _createdRequest.Created.Should().BeCloseTo(DateTime.UtcNow, 500);

            for (var i = 0; i < expectedSeasonCount; i++)
            {
                _createdRequest.Seasons[i].Should().BeEquivalentTo(_command.Seasons[i]);
            }

            var expectedAdditionalAgents = new List<MediaAgent>
            {
                new MediaAgent(AgentTypes.TheTvDb, _externalIds.TvDb_Id)
            };

            _createdRequest.AdditionalAgents.Should().BeEquivalentTo(expectedAdditionalAgents);
        }

        private void ThenTvSeasonDetailsWereRetrieved(int expectedSeasonCount)
        {
            _theMovieDbApi.Received(expectedSeasonCount).GetTvSeasonDetails(Arg.Any<int>(), Arg.Any<int>());
        }

        private List<PlexSeason> CreatePlexSeasonsFromCommand()
        {
            var plexSeasons = new List<PlexSeason>();

            foreach (var season in _command.Seasons)
            {
                var plexEpisodes = season.Episodes.Select(episode => new PlexEpisode
                {
                    Episode = episode.Index
                }).ToList();

                plexSeasons.Add(new PlexSeason
                {
                    Season = season.Index,
                    Episodes = plexEpisodes
                });
            }

            return plexSeasons.ToList();
        }
    }
}