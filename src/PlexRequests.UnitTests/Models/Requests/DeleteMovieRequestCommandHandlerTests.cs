using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess.Dtos;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class DeleteMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<DeleteMovieRequestCommand> _underTest;
        
        private DeleteMovieRequestCommand _command;
        private readonly IMovieRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsUserAccessor;

        private readonly Fixture _fixture;
        private Func<Task> _commandAction;
        private MovieRequestRow _request;

        public DeleteMovieRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<IMovieRequestService>();
            _claimsUserAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            
            _underTest = new DeleteMovieRequestCommandHandler(_requestService, _claimsUserAccessor);
            
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        private void Throws_Error_When_Request_Does_Not_Exist()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoRequestIsFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Invalid request id", "A request for the given id was not found.", HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Valid_Request_But_User_Is_Not_Requesting_User()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFound())
                .Given(x=> x.GivenRequestUserIsNotCurrentUser())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Unable to delete request", "Forbidden access to protected resource.", HttpStatusCode.Forbidden))
                .BDDfy();
        }

        [Fact]
        private void Request_Is_Deleted()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenRequestUserIsCurrentUser())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenDeleteIsSuccessful())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<DeleteMovieRequestCommand>();
        }

        private void GivenNoRequestIsFound()
        {
            _requestService.GetRequestById(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenARequestIsFound()
        {
            _request = _fixture.Create<MovieRequestRow>();
            
            _requestService.GetRequestById(Arg.Any<int>()).Returns(_request);
        }

        private void GivenRequestUserIsNotCurrentUser()
        {
            _claimsUserAccessor.UserId.Returns(_fixture.Create<int>());
        }

        private void GivenRequestUserIsCurrentUser()
        {
            _claimsUserAccessor.UserId.Returns(_request.UserId);
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

        private void ThenDeleteIsSuccessful()
        {
            _commandAction.Should().NotThrow();
            
            _requestService.Received().DeleteRequest(Arg.Is(_command.Id));
        }
    }
}