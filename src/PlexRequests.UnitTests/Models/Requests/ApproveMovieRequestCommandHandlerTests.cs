using System;
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
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class ApproveMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<ApproveMovieRequestCommand> _underTest;
        private readonly IRequestService _requestService;

        private readonly Fixture _fixture;

        private ApproveMovieRequestCommand _command;
        private Func<Task> _commandAction;
        private Request _updatedRequest;

        public ApproveMovieRequestCommandHandlerTests()
        {
            _fixture = new Fixture();

            _requestService = Substitute.For<IRequestService>();
            
            _underTest = new ApproveMovieRequestCommandHandler(_requestService);
        }

        [Fact]
        private void Throws_Error_When_Request_Does_Not_Exist()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoRequestIsFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "No request was found with the given Id", HttpStatusCode.NotFound))
                .BDDfy();
        }
        
        [Fact]
        private void Throws_Error_When_Request_Already_Completed()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFoundWithStatus(RequestStatuses.Completed))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "Request has already been completed", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Updates_Request_To_Approved_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFoundWithStatus(RequestStatuses.PendingApproval))
                .Given(x => x.GivenARequestIsUpdated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenTheCommandIsSuccessful())
                .Then(x => x.ThenARequestWasUpdatedCorrectly())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<ApproveMovieRequestCommand>();
        }

        private void GivenNoRequestIsFound()
        {
            _requestService.GetRequestById(Arg.Any<Guid>()).ReturnsNull();
        }

        private void GivenARequestIsFoundWithStatus(RequestStatuses status)
        {
            var request = _fixture.Build<Request>()
                                  .With(x => x.Status, status)
                                  .Create();
            
            _requestService.GetRequestById(Arg.Any<Guid>()).Returns(request);
        }

        private void GivenARequestIsUpdated()
        {
            _requestService.Update(Arg.Do<Request>(x => _updatedRequest = x));
        }

        private void WhenCommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenAnErrorIsThrown(string expectedMessage, string expectedDescription, HttpStatusCode statusCode)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(expectedMessage)
                          .Where(x => x.Description == expectedDescription)
                          .Where(x => x.StatusCode == statusCode);
        }

        private void ThenTheCommandIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenARequestWasUpdatedCorrectly()
        {
            _updatedRequest.Should().NotBeNull();
            _updatedRequest.Status.Should().Be(RequestStatuses.Approved);
        }
    }
}