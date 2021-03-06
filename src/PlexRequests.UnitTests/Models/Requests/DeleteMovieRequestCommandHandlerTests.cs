using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Requests.Commands;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class DeleteMovieRequestCommandHandlerTests
    {
        private readonly IRequestHandler<DeleteMovieRequestCommand, ValidationContext> _underTest;
        
        private DeleteMovieRequestCommand _command;
        private readonly IMovieRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        private Func<Task<ValidationContext>> _commandAction;
        private MovieRequestRow _request;
        private int _userId = 123;

        public DeleteMovieRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<IMovieRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            
            _underTest = new DeleteMovieRequestCommandHandler(_requestService, _unitOfWork);
        }

        [Fact]
        private void Throws_Error_When_Request_Does_Not_Exist()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoRequestIsFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Invalid request id", "A request for the given id was not found."))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Valid_Request_But_User_Is_Not_Requesting_User()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenRequestIsForDifferentUser())
                .Given(x => x.GivenARequestIsFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Unable to delete request", "Forbidden access to protected resource."))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void Request_Is_Deleted()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenDeleteIsSuccessful())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = new DeleteMovieRequestCommand(1)
            {
                UserInfo = new UserInfo
                {
                    UserId = _userId
                }
            };
        }

        private void GivenRequestIsForDifferentUser()
        {
            _command.UserInfo.UserId = int.MaxValue;
        }

        private void GivenNoRequestIsFound()
        {
            _requestService.GetRequestById(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenARequestIsFound()
        {
            _request = new MovieRequestRowBuilder().Build();
            _request.UserId = _userId;

            _requestService.GetRequestById(Arg.Any<int>()).Returns(_request);
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

        private void ThenDeleteIsSuccessful()
        {
            _commandAction.Should().NotThrow();
            
            _requestService.Received().DeleteRequest(Arg.Is(_command.Id));
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