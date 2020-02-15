using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.ApiRequests.Issues.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Issues
{
    public class CreateIssueCommandHandlerTests
    {
        private readonly IRequestHandler<CreateIssueCommand> _underTest;

        private readonly IIssueService _issueService;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private readonly Fixture _fixture;

        private CreateIssueCommand _command;
        private Func<Task> _commandAction;
        private IssueRow _issueCreated;
        private PlexMediaItemRow _plexMediaItem;
        private int _claimsUserId;


        public CreateIssueCommandHandlerTests()
        {
            _issueService = Substitute.For<IIssueService>();
            _plexService = Substitute.For<IPlexService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();

            _underTest = new CreateIssueCommandHandler(_issueService, _plexService, _unitOfWork, _claimsPrincipalAccessor);

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        private void Throws_Error_If_Title_Is_Empty(string title)
        {
            this.Given(x => x.GivenACommand())
            .Given(x => x.GivenATitle(title))
            .When(x => x.WhenCommandActionIsCreated())
            .Then(x => x.ThenErrorIsThrown("Issue not created", "'Title' must be specified", HttpStatusCode.BadRequest))
            .Then(x => x.ThenChangesAreNotCommitted())
            .BDDfy();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        private void Throws_Error_If_Description_Is_Empty(string description)
        {
            this.Given(x => x.GivenACommand())
            .Given(x => x.GivenADescription(description))
            .When(x => x.WhenCommandActionIsCreated())
            .Then(x => x.ThenErrorIsThrown("Issue not created", "'Description' must be specified", HttpStatusCode.BadRequest))
            .Then(x => x.ThenChangesAreNotCommitted())
            .BDDfy();
        }

        [Theory]
        [InlineData(PlexMediaTypes.Movie)]
        [InlineData(PlexMediaTypes.Show)]
        private void Creates_Issue_Successfully(PlexMediaTypes mediaType)
        {
            this.Given(x => x.GivenACommand())
            .Given(x => x.GivenAMediaType(mediaType))
            .Given(x => x.GivenMediaItemIsReturned())
            .Given(x => x.GivenAnIssueIsCreated())
            .Given(x => x.GivenUserDetailsFromClaims())
            .When(x => x.WhenCommandActionIsCreated())
            .Then(x => x.ThenIssueIsCreated())
            .Then(x => x.ThenChangesAreCommitted())
            .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<CreateIssueCommand>();
        }

        private void GivenAMediaType(PlexMediaTypes mediaType)
        {
            _command.MediaType = mediaType;
        }

        private void GivenATitle(string title)
        {
            _command.Title = title;
        }

        private void GivenADescription(string description)
        {
            _command.Description = description;
        }

        private void GivenMediaItemIsReturned()
        {
            _plexMediaItem = _fixture.Create<PlexMediaItemRow>();

            _plexService.GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(_plexMediaItem);

        }

        private void GivenAnIssueIsCreated()
        {
            _issueService.Add(Arg.Do<IssueRow>(x => _issueCreated = x));
        }

        private void GivenUserDetailsFromClaims()
        {
            _claimsUserId = _fixture.Create<int>();

            _claimsPrincipalAccessor.UserId.Returns(_claimsUserId);
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

        private void ThenIssueIsCreated()
        {
            _commandAction.Should().NotThrow();

            _issueCreated.Should().NotBeNull();
            _issueCreated.IssueId.Should().Be(default);
            _issueCreated.PlexMediaItem.Should().BeEquivalentTo(_plexMediaItem);
            _issueCreated.Title.Should().Be(_command.Title);
            _issueCreated.Description.Should().Be(_command.Description);
            _issueCreated.IssueStatus.Should().Be(IssueStatuses.Pending);
            _issueCreated.UserId.Should().Be(_claimsUserId);
            _issueCreated.IssueComments.Should().BeEmpty();
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