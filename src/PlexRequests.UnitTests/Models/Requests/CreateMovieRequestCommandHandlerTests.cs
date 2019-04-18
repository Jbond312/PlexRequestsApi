using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Models.Requests;
using PlexRequests.Plex;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class CreateMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<CreateMovieRequestCommand> _underTest;

        private readonly ITheMovieDbApi _theMovieDbApi;
        private readonly IRequestService _requestService;
        private readonly IPlexService _plexService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private readonly Fixture _fixture;
        
        private CreateMovieRequestCommand _command;
        private Request _request;
        private Func<Task> _commandAction;
        private PlexMediaItem _plexMediaItem;
        private Request _createdRequest;
        private string _claimsUsername;
        private Guid _claimsUserId;
        private MovieDetails _movieDetails;
        private ExternalIds _externalIds;

        public CreateMovieRequestCommandHandlerTests()
        {
            _theMovieDbApi = Substitute.For<ITheMovieDbApi>();
            _requestService = Substitute.For<IRequestService>();
            _plexService = Substitute.For<IPlexService>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            var logger = Substitute.For<ILogger<CreateRequestCommandHandler>>();

            _underTest = new CreateRequestCommandHandler(_theMovieDbApi, _requestService, _plexService, _claimsPrincipalAccessor, logger);

            _fixture = new Fixture();

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
            _request = _fixture.Create<Request>();

            _requestService.GetExistingMovieRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_request);
        }
        
        private void GivenNoRequestExists()
        {
            _requestService.GetExistingMovieRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).ReturnsNull();
        }

        private void GivenMovieAlreadyInPlex()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItem>();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }
        
        private void GivenMovieAlreadyInPlexFromFallbackAgent()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItem>();

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
            
            _theMovieDbApi.GetMovieDetails(Arg.Any<int>()).Returns(_movieDetails);
        }

        private void GivenARequestIsCreated()
        {
            _requestService.Create(Arg.Do<Request>(x => _createdRequest = x));
        }

        private void GivenExternalIdsFromTheMovieDb()
        {
            _externalIds = _fixture.Create<ExternalIds>(); 
            _theMovieDbApi.GetMovieExternalIds(Arg.Any<int>()).Returns(_externalIds);
        }

        private void GivenUserDetailsFromClaims()
        {
            _claimsUsername = _fixture.Create<string>();
            _claimsUserId = _fixture.Create<Guid>();

            _claimsPrincipalAccessor.Username.Returns(_claimsUsername);
            _claimsPrincipalAccessor.UserId.Returns(_claimsUserId);
        }

        private void ThenRequestIsCreated()
        {
            _commandAction.Should().NotThrow();

            _createdRequest.Should().NotBeNull();
            _createdRequest.Id.Should().Be(Guid.Empty);
            _createdRequest.MediaType.Should().Be(PlexMediaTypes.Movie);
            _createdRequest.Status.Should().Be(RequestStatuses.PendingApproval);
            _createdRequest.PrimaryAgent.AgentType.Should().Be(AgentTypes.TheMovieDb);
            _createdRequest.PrimaryAgent.AgentSourceId.Should().Be(_command.TheMovieDbId.ToString());
            _createdRequest.PlexMediaUri.Should().BeNull();
            _createdRequest.Seasons.Should().BeNull();
            _createdRequest.RequestedByUserName.Should().Be(_claimsUsername);
            _createdRequest.RequestedByUserId.Should().Be(_claimsUserId);
            _createdRequest.Title.Should().Be(_movieDetails.Title);
            _createdRequest.ImagePath.Should().Be(_movieDetails.Poster_Path);
            _createdRequest.AirDate.Should().Be(DateTime.Parse(_movieDetails.Release_Date));
            _createdRequest.Created.Should().BeCloseTo(DateTime.UtcNow, 500);

            var additionalAgent = new RequestAgent(AgentTypes.Imdb, _externalIds.Imdb_Id);

            _createdRequest.AdditionalAgents.Should().BeEquivalentTo(new List<RequestAgent> {additionalAgent});

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