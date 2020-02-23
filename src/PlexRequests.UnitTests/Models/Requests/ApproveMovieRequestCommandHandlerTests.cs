using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class ApproveMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<ApproveMovieRequestCommand> _underTest;
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        private ApproveMovieRequestCommand _command;
        private Func<Task> _commandAction;
        private MovieRequestRow _requestToUpdate;

        public ApproveMovieRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<IMovieRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            
            _underTest = new ApproveMovieRequestCommandHandler(_requestService, _unitOfWork);
        }

        [Fact]
        private void Throws_Error_When_Request_Does_Not_Exist()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoRequestIsFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "No request was found with the given Id", HttpStatusCode.NotFound))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }
        
        [Fact]
        private void Throws_Error_When_Request_Already_Completed()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFoundWithStatus(RequestStatuses.Completed))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "Request has already been completed", HttpStatusCode.BadRequest))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void Updates_Request_To_Approved_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFoundWithStatus(RequestStatuses.PendingApproval))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenTheCommandIsSuccessful())
                .Then(x => x.ThenARequestWasUpdatedCorrectly())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = new ApproveMovieRequestCommand(1);
        }

        private void GivenNoRequestIsFound()
        {
            _requestService.GetRequestById(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenARequestIsFoundWithStatus(RequestStatuses status)
        {
            _requestToUpdate = new MovieRequestRowBuilder().WithRequestStatus(status).Build();
            _requestService.GetRequestById(Arg.Any<int>()).Returns(_requestToUpdate);
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
            _requestToUpdate.Should().NotBeNull();
            _requestToUpdate.RequestStatus.Should().Be(RequestStatuses.Approved);
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