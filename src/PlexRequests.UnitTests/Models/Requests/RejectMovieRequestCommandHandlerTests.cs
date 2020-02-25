using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.ApiRequests;
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
    public class RejectMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<RejectMovieRequestCommand, ValidationContext> _underTest;
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        private RejectMovieRequestCommand _command;
        private Func<Task<ValidationContext>> _commandAction;
        private MovieRequestRow _request;

        public RejectMovieRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<IMovieRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            
            _underTest = new RejectMovieRequestCommandHandler(_requestService, _unitOfWork);
        }

        [Fact]
        private void Throws_Error_When_Request_Does_Not_Exist()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoRequestFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "No request was found with the given Id"))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Request_Already_Complete()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestWithStatus(RequestStatuses.Completed))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "Request has already been completed"))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        private void Throws_Error_When_No_Comment_Specified(string comment)
        {
            this.Given(x => x.GivenACommandWithComment(comment))
                .Given(x => x.GivenARequestWithStatus(RequestStatuses.Approved))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "A comment must be specified when rejecting a request"))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void Updates_Request_Correctly()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestWithStatus(RequestStatuses.Approved))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestUpdatedCorrectly())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = new RejectMovieRequestCommand(1, Guid.NewGuid().ToString());
        }
        
        private void GivenACommandWithComment(string comment)
        {
            _command = new RejectMovieRequestCommand(1, comment);
        }

        private void GivenNoRequestFound()
        {
            _requestService.GetRequestById(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenARequestWithStatus(RequestStatuses status)
        {
            _request = new MovieRequestRowBuilder().WithRequestStatus(status).Build();
            _requestService.GetRequestById(Arg.Any<int>()).Returns(_request);
        }

        private void WhenCommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private async Task ThenAnErrorIsThrown(string expectedMessage, string expectedDescription)
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeFalse();
            var firstError = result.ValidationErrors[0];
            firstError.Message.Should().Be(expectedMessage);
            firstError.Description.Should().Be(expectedDescription);
        }

        private async Task ThenRequestUpdatedCorrectly()
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeTrue();
            
            _request.Should().NotBeNull();
            _request.RequestStatus.Should().Be(RequestStatuses.Rejected);
            _request.Comment.Should().Be(_command.Comment);
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