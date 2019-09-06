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
    public class RejectMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<RejectMovieRequestCommand> _underTest;
        private readonly IMovieRequestService _requestService;

        private readonly Fixture _fixture;

        private RejectMovieRequestCommand _command;
        private Func<Task> _commandAction;
        private MovieRequest _request;
        private MovieRequest _updatedRequest;

        public RejectMovieRequestCommandHandlerTests()
        {
            _fixture = new Fixture();

            _requestService = Substitute.For<IMovieRequestService>();
            
            _underTest = new RejectMovieRequestCommandHandler(_requestService);
        }

        [Fact]
        private void Throws_Error_When_Request_Does_Not_Exist()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoRequestFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "No request was found with the given Id", HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Request_Already_Complete()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestWithStatus(RequestStatuses.Completed))
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "Request has already been completed", HttpStatusCode.BadRequest))
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
                .Then(x => x.ThenAnErrorIsThrown("Invalid comment", "A comment must be specified when rejecting a request", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Updates_Request_Correctly()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestWithStatus(RequestStatuses.Approved))
                .Given(x => x.GivenARequestIsUpdated())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenRequestUpdatedCorrectly())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<RejectMovieRequestCommand>();
        }
        
        private void GivenACommandWithComment(string comment)
        {
            _command = _fixture.Build<RejectMovieRequestCommand>()
                               .With(x => x.Comment, comment)
                               .Create();
        }

        private void GivenNoRequestFound()
        {
            _requestService.GetRequestById(Arg.Any<Guid>()).ReturnsNull();
        }

        private void GivenARequestWithStatus(RequestStatuses status)
        {
            _request = _fixture.Build<MovieRequest>()
                               .With(x => x.Status, status)
                               .Create();

            _requestService.GetRequestById(Arg.Any<Guid>()).Returns(_request);
        }

        private void GivenARequestIsUpdated()
        {
            _requestService.Update(Arg.Do<MovieRequest>(x => _updatedRequest = x));
        }

        private void WhenCommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenAnErrorIsThrown(string expectedMessage, string expectedDescription,
            HttpStatusCode expectedStatusCode)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(expectedMessage)
                          .Where(x => x.Description == expectedDescription)
                          .Where(x => x.StatusCode == expectedStatusCode);
        }

        private void ThenRequestUpdatedCorrectly()
        {
            _commandAction.Should().NotThrow();

            _updatedRequest.Should().NotBeNull();
            _updatedRequest.Status.Should().Be(RequestStatuses.Rejected);
            _updatedRequest.Comment.Should().Be(_command.Comment);
        }
    }
}