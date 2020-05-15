using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Requests.Commands;
using PlexRequests.Plex;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class CreateMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<CreateMovieRequestCommand, ValidationContext> _underTest;

        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieRequestService _requestService;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;

        private CreateMovieRequestCommand _command;
        private MovieRequestRow _request;
        private Func<Task<ValidationContext>> _commandAction;
        private PlexMediaItemRow _plexMediaItem;
        private MovieRequestRow _createdRequest;
        private MovieDetails _movieDetails;
        private ExternalIds _externalIds;

        public CreateMovieRequestCommandHandlerTests()
        {
            _theMovieDbService = Substitute.For<ITheMovieDbService>();
            _requestService = Substitute.For<IMovieRequestService>();
            _plexService = Substitute.For<IPlexService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            var logger = Substitute.For<ILogger<CreateRequestCommandHandler>>();

            _underTest = new CreateRequestCommandHandler(_theMovieDbService, _requestService, _plexService, _unitOfWork, logger);

        }

        [Fact]
        private void Throws_Error_If_Movie_Already_Requested()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => GivenMovieIsInTheMovieDb())
                .Given(x => x.GivenRequestAlreadyExists())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Request not created", "The Movie has already been requested."))
                .Then(x => x.ThenChangesAreNotCommitted())
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
                .Then(x => x.ThenErrorIsThrown("Request not created", "The Movie is already available in Plex."))
                .Then(x => x.ThenChangesAreNotCommitted())
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
                .Then(x => x.ThenErrorIsThrown("Request not created", "The Movie is already available in Plex."))
                .Then(x => x.ThenChangesAreNotCommitted())
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
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestIsCreated())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = new CreateMovieRequestCommand
            {
                UserInfo = new UserInfo()
            };
        }

        private void GivenRequestAlreadyExists()
        {

            _request = new MovieRequestRowBuilder().Build();

            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_request);
        }

        private void GivenNoRequestExists()
        {
            _requestService.GetExistingRequest(Arg.Any<AgentTypes>(), Arg.Any<string>()).ReturnsNull();
        }

        private void GivenMovieAlreadyInPlex()
        {
            _plexMediaItem = new MoviePlexMediaItemRowBuilder().Build();

            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(_plexMediaItem);
        }

        private void GivenMovieAlreadyInPlexFromFallbackAgent()
        {
            _plexMediaItem = new MoviePlexMediaItemRowBuilder().Build();

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
            _movieDetails = new MovieDetailsBuilder().WithReleaseDate("2019-12-25").Build();
            _theMovieDbService.GetMovieDetails(Arg.Any<int>()).Returns(_movieDetails);
        }

        private void GivenARequestIsCreated()
        {
            _requestService.Add(Arg.Do<MovieRequestRow>(x => _createdRequest = x));
        }

        private void GivenExternalIdsFromTheMovieDb()
        {
            _externalIds = new ExternalIdsBuilder().Build();
            _theMovieDbService.GetMovieExternalIds(Arg.Any<int>()).Returns(_externalIds);
        }

        private async Task ThenRequestIsCreated()
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeTrue();

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

        private async Task ThenErrorIsThrown(string message, string description)
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeFalse();
            var firstError = result.ValidationErrors[0];
            firstError.Message.Should().Be(message);
            firstError.Description.Should().Be(description);
        }

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }

        private void ThenChangesAreNotCommitted()
        {
            _unitOfWork.DidNotReceive().CommitAsync();
        }
    }
}