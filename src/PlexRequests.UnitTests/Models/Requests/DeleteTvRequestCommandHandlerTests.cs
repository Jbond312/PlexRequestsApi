using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core.Helpers;
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
    public class DeleteTvRequestCommandHandlerTests
    {
        private readonly IRequestHandler<DeleteTvRequestCommand, ValidationContext> _underTest;
        
        private DeleteTvRequestCommand _command;
        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsUserAccessor;

        private Func<Task<ValidationContext>> _commandAction;
        private TvRequestRow _request;
        private readonly int _requestUserId;

        public DeleteTvRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<ITvRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _claimsUserAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            var logger = Substitute.For<ILogger<DeleteTvRequestCommandHandler>>();
            
            _underTest = new DeleteTvRequestCommandHandler(_requestService, _unitOfWork, _claimsUserAccessor, logger);
            
            _requestUserId = new Random().Next(1, int.MaxValue);
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
        private void Request_Is_Deleted_When_No_User_Requests_Remaining()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenOnlyASingleUserHasRequestedTvShow())
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
            _command = new DeleteTvRequestCommand(1);
        }

        private void GivenNoRequestIsFound()
        {
            _requestService.GetRequestById(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenARequestIsFound()
        {
            _request = new TvRequestRowBuilder().Build();
            _requestService.GetRequestById(Arg.Any<int>()).Returns(_request);
        }

        private void GivenMultipleUsersHaveRequestedTvShow()
        {
            var requestUser = new TvRequestUserRowBuilder().WithUserId(_requestUserId).Build();
            var otherRequestUser = new TvRequestUserRowBuilder().Build();

            _request.TvRequestUsers = new List<TvRequestUserRow>
            {
                requestUser,
                otherRequestUser
            };
        }

        private void GivenOnlyASingleUserHasRequestedTvShow()
        {
            _request.TvRequestUsers = new List<TvRequestUserRow>();
            var requestUser = new TvRequestUserRowBuilder().WithUserId(_requestUserId).Build();

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

        private async Task ThenErrorIsThrown(string message, string description)
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeFalse();
            var firstError = result.ValidationErrors[0];
            firstError.Message.Should().Be(message);
            firstError.Description.Should().Be(description);
        }

        private async Task ThenCommandIsSuccessful()
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeTrue();
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