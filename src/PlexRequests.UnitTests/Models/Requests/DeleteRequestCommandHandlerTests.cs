using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Models.Requests;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class DeleteRequestCommandHandlerTests
    {
        private readonly IRequestHandler<DeleteRequestCommand> _underTest;
        
        private DeleteRequestCommand _command;
        private readonly IRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsUserAccessor;

        private readonly Fixture _fixture;
        private Func<Task> _commandAction;
        private Request _request;

        public DeleteRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<IRequestService>();
            _claimsUserAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            
            _underTest = new DeleteRequestCommandHandler(_requestService, _claimsUserAccessor);
            
            _fixture = new Fixture();
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
            _command = _fixture.Create<DeleteRequestCommand>();
        }

        private void GivenNoRequestIsFound()
        {
            _requestService.GetRequestById(Arg.Any<Guid>()).ReturnsNull();
        }

        private void GivenARequestIsFound()
        {
            _request = _fixture.Create<Request>();
            
            _requestService.GetRequestById(Arg.Any<Guid>()).Returns(_request);
        }

        private void GivenRequestUserIsNotCurrentUser()
        {
            _claimsUserAccessor.UserId.Returns(_fixture.Create<Guid>());
        }

        private void GivenRequestUserIsCurrentUser()
        {
            _claimsUserAccessor.UserId.Returns(_request.RequestedByUserId);
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