using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class CreateMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<CreateMovieRequestCommand> _underTest;

        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieRequestService _requestService;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private readonly Fixture _fixture;

        private CreateMovieRequestCommand _command;
        private MovieRequestRow _request;
        private Func<Task> _commandAction;
        private PlexMediaItemRow _plexMediaItem;
        private MovieRequestRow _createdRequest;
        private string _claimsUsername;
        private int _claimsUserId;
        private MovieDetails _movieDetails;
        private ExternalIds _externalIds;

        public CreateMovieRequestCommandHandlerTests()
        {
            _theMovieDbService = Substitute.For<ITheMovieDbService>();
            _requestService = Substitute.For<IMovieRequestService>();
            _plexService = Substitute.For<IPlexService>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            var logger = Substitute.For<ILogger<CreateRequestCommandHandler>>();

            _underTest = new CreateRequestCommandHandler(_theMovieDbService, _requestService, _plexService, _claimsPrincipalAccessor, logger);

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        }

        [Fact]
        private void Throws_Error_If_Movie_Already_Requested()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => GivenMovieIsInTheMovieDb())
                .Given(x => x.GivenRequestAlreadyExists())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created", "The Movie has already been requested.",
                    HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_If_Movie_Already_In_Plex_From_Primary_Agent_TheMovieDb()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => GivenMovieIsInTheMovieDb())
                .Given(x => x.GivenNoRequestExists())
                .Given(x => x.GivenMovieAlreadyInPlex())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created", "The Movie is already available in Plex.",
                    HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_If_Movie_Already_In_Plex_From_Fallback_Agent_Imdb()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => GivenMovieIsInTheMovieDb())
                .Given(x => x.GivenNoRequestExists())
                .Given(x => x.GivenExternalIdsFromTheMovieDb())
                .Given(x => x.GivenMovieAlreadyInPlexFromFallbackAgent())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created", "The Movie is already available in Plex.",
                    HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Creates_Request_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenExternalIdsFromTheMovieDb())
                .Given(x => GivenMovieIsInTheMovieDb())
                .Given(x => x.GivenNoRequestExists())
                .Given(x => x.GivenMovieNotInPlex())
                .Given(x => x.GivenARequestIsCreated())
                .Given(x => x.GivenUserDetailsFromClaims())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<CreateMovieRequestCommand>();
        }

        private void GivenRequestAlreadyExists()
        {
            _request = _fixture.Create<MovieRequestRow>();

            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_request);
        }

        private void GivenNoRequestExists()
        {
            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).ReturnsNull();
        }

        private void GivenMovieAlreadyInPlex()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItemRow>();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }

        private void GivenMovieAlreadyInPlexFromFallbackAgent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItemRow>();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(null, _plexMediaItem);
        }

        private void GivenMovieNotInPlex()
        {
            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .ReturnsNull();
        }

        private void GivenMovieIsInTheMovieDb()
        {
            _movieDetails = _fixture.Build<MovieDetails>()
                                    .With(x => x.Release_Date, "2019-12-25")
                                    .Create();

            _theMovieDbService.GetMovieDetails(Arg.Any<int>()).Returns(_movieDetails);
        }

        private void GivenARequestIsCreated()
        {
            _requestService.Add(Arg.Do<MovieRequestRow>(x => _createdRequest = x));
        }

        private void GivenExternalIdsFromTheMovieDb()
        {
            _externalIds = _fixture.Create<ExternalIds>();
            _theMovieDbService.GetMovieExternalIds(Arg.Any<int>()).Returns(_externalIds);
        }

        private void GivenUserDetailsFromClaims()
        {
            _claimsUsername = _fixture.Create<string>();
            _claimsUserId = _fixture.Create<int>();

            _claimsPrincipalAccessor.Username.Returns(_claimsUsername);
            _claimsPrincipalAccessor.UserId.Returns(_claimsUserId);
        }

        private void ThenRequestIsCreated()
        {
            _commandAction.Should().NotThrow();

            _createdRequest.Should().NotBeNull();
            _createdRequest.MovieRequestId.Should().Be(default);
            _createdRequest.RequestStatus.Should().Be(RequestStatuses.PendingApproval);
            _createdRequest.PrimaryAgent.AgentType.Should().Be(AgentTypes.TheMovieDb);
            _createdRequest.PrimaryAgent.AgentSourceId.Should().Be(_command.TheMovieDbId.ToString());
            _createdRequest.PlexMediaItem.Should().BeNull();
            _createdRequest.Title.Should().Be(_movieDetails.Title);
            _createdRequest.ImagePath.Should().Be(_movieDetails.Poster_Path);
            _createdRequest.AirDateUtc.Should().Be(DateTime.Parse(_movieDetails.Release_Date));
            _createdRequest.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, 500);

            var additionalAgent = new MovieRequestAgentRow(AgentTypes.Imdb, _externalIds.Imdb_Id);

            var otherAgents = _createdRequest.MovieRequestAgents.Where(x => x != _createdRequest.PrimaryAgent);

            otherAgents.Should().BeEquivalentTo(new List<MovieRequestAgentRow> { additionalAgent }, options => options.Excluding(x => x.CreatedUtc));
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
    }
}