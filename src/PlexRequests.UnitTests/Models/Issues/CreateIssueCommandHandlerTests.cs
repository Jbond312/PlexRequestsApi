using System;
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
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Issues
{
    public class CreateIssueCommandHandlerTests
    {
        private readonly IRequestHandler<CreateIssueCommand> _underTest;

        private IIssueService _issueService;
        private ITheMovieDbApi _theMovieDbApi;
        private IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private readonly Fixture _fixture;

        private CreateIssueCommand _command;
        private Func<Task> _commandAction;
        private MovieDetails _movieDetails;
        private TvDetails _tvDetails;
        private Issue _issueCreated;
        private string _claimsUsername;
        private Guid _claimsUserId;


        public CreateIssueCommandHandlerTests()
        {
            _issueService = Substitute.For<IIssueService>();
            _theMovieDbApi = Substitute.For<ITheMovieDbApi>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();

            _underTest = new CreateIssueCommandHandler(_issueService, _theMovieDbApi, _claimsPrincipalAccessor);

            _fixture = new Fixture();
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
            .BDDfy();
        }

        [Theory]
        [InlineData(PlexMediaTypes.Movie)]
        [InlineData(PlexMediaTypes.Show)]
        private void Creates_Issue_Successfully(PlexMediaTypes mediaType)
        {
            this.Given(x => x.GivenACommand())
            .Given(x => x.GivenAMediaType(mediaType))
            .Given(x => x.GivenMediaItemsAreReturned())
            .Given(x => x.GivenAnIssueIsCreated())
            .Given(x => x.GivenUserDetailsFromClaims())
            .When(x => x.WhenCommandActionIsCreated())
            .Then(x => x.ThenIssueIsCreated())
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

        private void GivenMediaItemsAreReturned()
        {
            if (_command.MediaType == PlexMediaTypes.Movie)
            {
                _movieDetails = _fixture.Build<MovieDetails>()
                                        .With(x => x.Release_Date, "2019-12-25")
                                        .Create();

                _theMovieDbApi.GetMovieDetails(Arg.Any<int>()).Returns(_movieDetails);
            }
            else
            {
                _tvDetails = _fixture.Build<TvDetails>()
                     .With(x => x.First_Air_Date, "2019-12-25")
                     .Create();

                _theMovieDbApi.GetTvDetails(Arg.Any<int>()).Returns(_tvDetails);
            }
        }

        private void GivenAnIssueIsCreated()
        {
            _issueService.Create(Arg.Do<Issue>(x => _issueCreated = x));
        }

        private void GivenUserDetailsFromClaims()
        {
            _claimsUsername = _fixture.Create<string>();
            _claimsUserId = _fixture.Create<Guid>();

            _claimsPrincipalAccessor.Username.Returns(_claimsUsername);
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
            _issueCreated.Id.Should().Be(Guid.Empty);
            _issueCreated.MediaType.Should().Be(_command.MediaType);
            _issueCreated.MediaAgent.Should().Be(new MediaAgent(AgentTypes.TheMovieDb, _command.TheMovieDbId.ToString()));
            _issueCreated.Title.Should().Be(_command.Title);
            _issueCreated.Description.Should().Be(_command.Description);
            _issueCreated.Status.Should().Be(IssueStatuses.Pending);
            _issueCreated.RequestedByUserId.Should().Be(_claimsUserId);
            _issueCreated.RequestedByUserName.Should().Be(_claimsUsername);
            _issueCreated.Created.Should().BeCloseTo(DateTime.UtcNow, 500);
            _issueCreated.Comments.Should().BeEmpty();

            string mediaItemName;
            string imagePath;
            DateTime airDate;
            if (_command.MediaType == PlexMediaTypes.Movie)
            {
                mediaItemName = _movieDetails.Title;
                imagePath = _movieDetails.Poster_Path;
                airDate = DateTime.Parse(_movieDetails.Release_Date);
            }
            else
            {
                mediaItemName = _tvDetails.Name;
                imagePath = _tvDetails.Poster_Path;
                airDate = DateTime.Parse(_tvDetails.First_Air_Date);
            }

            _issueCreated.MediaItemName.Should().Be(mediaItemName);
            _issueCreated.ImagePath.Should().Be(imagePath);
            _issueCreated.AirDate.Should().Be(airDate);
        }
    }
}