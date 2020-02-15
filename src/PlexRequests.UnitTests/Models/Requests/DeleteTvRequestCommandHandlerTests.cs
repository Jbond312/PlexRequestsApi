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
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class DeleteTvRequestCommandHandlerTests
    {
        private readonly IRequestHandler<DeleteTvRequestCommand> _underTest;
        
        private DeleteTvRequestCommand _command;
        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsUserAccessor;

        private readonly Fixture _fixture;
        private Func<Task> _commandAction;
        private TvRequestRow _request;
        private readonly int _requestUserId;

        public DeleteTvRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<ITvRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _claimsUserAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            var logger = Substitute.For<ILogger<DeleteTvRequestCommandHandler>>();
            
            _underTest = new DeleteTvRequestCommandHandler(_requestService, _unitOfWork, _claimsUserAccessor, logger);
            
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _requestUserId = _fixture.Create<int>();
        }

        [Fact]
        private void Throws_Error_When_Request_Does_Not_Exist()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoRequestIsFound())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenErrorIsThrown("Invalid request id", "A request for the given id was not found.", HttpStatusCode.NotFound))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void Request_Is_Deleted_When_No_User_Requests_Remaining()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenUserHasRequestedTvShow())
                .Given(x => x.GivenRequestUserIsCurrentUser())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenEntireRequestIsDeleted())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void Request_User_Is_Deleted()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenMultipleUsersHaveRequestedTvShow())
                .Given(x => x.GivenRequestUserIsCurrentUser())
                .When(x => x.WhenCommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenEntireRequestIsNotDeleted())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<DeleteTvRequestCommand>();
        }

        private void GivenNoRequestIsFound()
        {
            _requestService.GetRequestById(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenARequestIsFound()
        {
            _request = _fixture.Build<TvRequestRow>()
                .With(x => x.TvRequestUsers, new List<TvRequestUserRow>())
                .Create();
            
            _requestService.GetRequestById(Arg.Any<int>()).Returns(_request);
        }

        private void GivenMultipleUsersHaveRequestedTvShow()
        {
            var requestUser = _fixture.Build<TvRequestUserRow>().With(x => x.UserId, _requestUserId).Create();
            var otherRequestUser = _fixture.Build<TvRequestUserRow>().Create();

            _request.TvRequestUsers = new List<TvRequestUserRow>
            {
                requestUser,
                otherRequestUser
            };
        }

        private void GivenUserHasRequestedTvShow()
        {
            var requestUser = _fixture.Build<TvRequestUserRow>().With(x => x.UserId, _requestUserId).Create();

            _request.TvRequestUsers.Add(requestUser);
        }
        
        private void GivenRequestUserIsCurrentUser()
        {
            _claimsUserAccessor.UserId.Returns(_requestUserId);
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

        private void ThenCommandIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenEntireRequestIsDeleted()
        {
            _requestService.Received().DeleteRequest(Arg.Is(_command.Id));
        }

        private void ThenEntireRequestIsNotDeleted()
        {
            _requestService.DidNotReceive().DeleteRequest(Arg.Is(_command.Id));
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