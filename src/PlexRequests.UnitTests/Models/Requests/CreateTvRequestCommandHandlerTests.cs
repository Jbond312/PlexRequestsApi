using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using MediatR;
using NSubstitute;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Mapping;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class CreateTvRequestCommandHandlerTests
    {
        private readonly IRequestHandler<CreateTvRequestCommand> _underTest;

        private readonly ITvRequestService _requestService;
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private readonly Fixture _fixture;

        private Func<Task> _commandAction;
        private CreateTvRequestCommand _command;
        private List<TvRequestRow> _requests;
        private PlexMediaItemRow _plexMediaItem;
        private TvRequestRow _createdRequest;
        private string _claimsUsername;
        private int _claimsUserId;
        private TvDetails _tvDetails;
        private TvSeasonDetails _season;
        private ExternalIds _externalIds;

        public CreateTvRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<ITvRequestService>();
            _theMovieDbService = Substitute.For<ITheMovieDbService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();

            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new RequestProfile()); });
            var mapper = mapperConfig.CreateMapper();

            _underTest = new CreateTvRequestCommandHandler(mapper, _requestService, _theMovieDbService, _unitOfWork, _claimsPrincipalAccessor);

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        /*
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
        private void Throws_Error_When_TrackShow_But_Show_Not_In_Production()
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
        private void Throws_Error_When_TrackShow_But_Show_Already_Tracked()
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
                .Then(x => x.ThenChangesAreCommitted())
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
                .Then(x => x.ThenChangesAreCommitted())
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
                .Then(x => x.ThenChangesAreCommitted())
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

            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }

        private void GivenOneSeasonNotAlreadyRequested()
        {
            CreateRequestsFromCommand();

            var firstRequest = _requests.First();
            var elementToRemove = firstRequest.TvRequestSeasons.ElementAt(0);
            firstRequest.TvRequestSeasons.Remove(elementToRemove);

            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }

        private void GivenShowAlreadyBeingTracked()
        {
            var existingRequest = _fixture.Build<TvRequestRow>()
            .With(x => x.Track, true)
            .Create();

            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(new List<TvRequestRow> { existingRequest });
        }

        private void CreateRequestsFromCommand()
        {
            var request = _fixture.Create<TvRequestRow>();

            request.TvRequestSeasons = new List<TvRequestSeasonRow>();
            foreach (var season in _command.Seasons)
            {
                var requestEpisodes = season.Episodes.Select(episode => new TvRequestEpisodeRow
                {
                    EpisodeIndex = episode.Index
                });

                var requestSeason = new TvRequestSeasonRow
                {
                    SeasonIndex = season.Index,
                    TvRequestEpisodes = requestEpisodes.ToList()
                };

                request.TvRequestSeasons.Add(requestSeason);
            }

            _requests = new List<TvRequestRow> { request };
        }

        private void GivenAllEpisodesAlreadyInPlex()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItemRow>();

            _plexMediaItem.PlexSeasons = CreatePlexSeasonsFromCommand();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }

        private void GivenAllEpisodesAlreadyInPlexFromFallbackAgent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItemRow>();

            _plexMediaItem.PlexSeasons = CreatePlexSeasonsFromCommand();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(null, _plexMediaItem);
        }

        private void GivenOneSeasonNotMatchingPlexContent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItemRow>();

            _plexMediaItem.PlexSeasons = CreatePlexSeasonsFromCommand();
            var elementToRemove = _plexMediaItem.PlexSeasons.ElementAt(0);
            _plexMediaItem.PlexSeasons.Remove(elementToRemove);

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }

        private void GivenNoMatchingRequests()
        {
            _requests = _fixture.CreateMany<TvRequestRow>().ToList();

            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_requests);
        }

        private void GivenNoMatchingPlexContent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItemRow>();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }

        private void GivenTheTvDbExternalIdReturned()
        {
            _externalIds = _fixture.Create<ExternalIds>();

            _theMovieDbService.GetTvExternalIds(Arg.Any<int>()).Returns(_externalIds);
        }

        private void GivenTheTvDetailsReturnedFromTheMovieDb()
        {
            _tvDetails = _fixture.Build<TvDetails>()
                                 .With(x => x.First_Air_Date, "2019-12-25")
                                 .Create();

            _theMovieDbService.GetTvDetails(_command.TheMovieDbId).Returns(_tvDetails);
        }

        private void GivenTvDetailsInProduction(bool inProduction)
        {
            _tvDetails.In_Production = inProduction;
        }

        private void GivenSeasonDetailsReturnedFromTheMovieDb()
        {
            _season = _fixture.Create<TvSeasonDetails>();

            _theMovieDbService.GetTvSeasonDetails(Arg.Any<int>(), Arg.Any<int>()).Returns(_season);

        }

        private void GivenARequestIsCreated()
        {
            _createdRequest = null;
            _requestService.Add(Arg.Do<TvRequestRow>(x => _createdRequest = x));
        }

        private void GivenUserDetailsFromClaims()
        {
            _claimsUsername = _fixture.Create<string>();
            _claimsUserId = _fixture.Create<int>();
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
            _requestService.Received().Add(Arg.Any<TvRequestRow>());

            _createdRequest.Should().NotBeNull();
            _createdRequest.TvRequestId.Should().Be(default(int));
            _createdRequest.RequestStatus.Should().Be(RequestStatuses.PendingApproval);
            _createdRequest.PrimaryAgent.AgentType.Should().Be(AgentTypes.TheMovieDb);
            _createdRequest.PrimaryAgent.AgentSourceId.Should().Be(_command.TheMovieDbId.ToString());
            _createdRequest.PlexMediaItem.MediaUri.Should().BeNull();
            _createdRequest.TvRequestSeasons.Should().NotBeNull();
            _createdRequest.TvRequestSeasons.Count.Should().Be(expectedSeasonCount);
            _createdRequest.Title.Should().Be(_tvDetails.Name);
            _createdRequest.AirDateUtc.Should().Be(DateTime.Parse(_tvDetails.First_Air_Date));
            _createdRequest.ImagePath.Should().Be(_tvDetails.Poster_Path);
            _createdRequest.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, 500);

            for (var i = 0; i < expectedSeasonCount; i++)
            {
                _createdRequest.TvRequestSeasons.ElementAt(i).Should().BeEquivalentTo(_command.Seasons[i]);
            }

            var expectedAdditionalAgents = new List<TvRequestAgentRow>
            {
                new TvRequestAgentRow(AgentTypes.TheTvDb, _externalIds.TvDb_Id)
            };

            _createdRequest.TvRequestAgents.Should().BeEquivalentTo(expectedAdditionalAgents);
        }

        private void ThenTvSeasonDetailsWereRetrieved(int expectedSeasonCount)
        {
            _theMovieDbService.Received(expectedSeasonCount).GetTvSeasonDetails(Arg.Any<int>(), Arg.Any<int>());
        }

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }

        private List<PlexSeasonRow> CreatePlexSeasonsFromCommand()
        {
            var plexSeasons = new List<PlexSeasonRow>();

            foreach (var season in _command.Seasons)
            {
                var plexEpisodes = season.Episodes.Select(episode => new PlexEpisodeRow
                {
                    Episode = episode.Index
                }).ToList();

                plexSeasons.Add(new PlexSeasonRow
                {
                    Season = season.Index,
                    PlexEpisodes = plexEpisodes
                });
            }

            return plexSeasons.ToList();
        }
    }
    */
    }
}