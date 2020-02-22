using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using PlexRequests.ApiRequests.Auth.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Plex;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Auth
{
    public class UserLoginCommandHandlerTests
    {
        private readonly UserLoginCommandHandler _underTest;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlexApi _plexApi;

        private UserLoginCommand _command;
        private UserRow _matchingDbUser;
        private string _createdToken;
        private UserRefreshTokenRow _createdRefreshToken;
        private Func<Task<UserLoginCommandResult>> _commandAction;

        public UserLoginCommandHandlerTests()
        {
            _userService = Substitute.For<IUserService>();
            _tokenService = Substitute.For<ITokenService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plexApi = Substitute.For<IPlexApi>();
            var logger = Substitute.For<ILogger<UserLoginCommandHandler>>();

            _underTest = new UserLoginCommandHandler(_userService, _tokenService, _unitOfWork, _plexApi, logger);
        }

        [Fact]
        private void Throws_Error_When_Invalid_PlexCredentials()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenInvalidPlexCredentialsThrowsException())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid Plex Credentials", "Unable to login to Plex with the given credentials", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_No_Matching_User_In_Database()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenNoMatchingUser())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Unrecognised user", "The user is not recognised or has been disabled.", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        public void Throws_Error_When_Matching_User_Is_Disabled()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAMatchingUser(true))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Unrecognised user", "The user is not recognised or has been disabled.", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void User_Is_Updated_With_New_LastLogin_Date()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAMatchingUser(false))
                .Given(x => x.GivenARefreshTokenIsCreated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenUserIsUpdatedCorrectly())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void Token_Is_Created_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAMatchingUser(false))
                .Given(x => x.GivenATokenIsCreated())
                .Given(x => x.GivenARefreshTokenIsCreated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenATokenIsReturnedCorrectly())
                .BDDfy();
        }

        [Fact]
        private void RefreshToken_Is_Created_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAMatchingUser(false))
                .Given(x => x.GivenARefreshTokenIsCreated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenARefreshTokenIsReturnedCorrectly())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = new UserLoginCommand
            {
                Username = "username",
                Password = "password"
            };
        }

        private void GivenInvalidPlexCredentialsThrowsException()
        {
            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).Throws(new PlexRequestException("", "", HttpStatusCode.FailedDependency));
        }


        private void GivenValidPlexCredentials()
        {
            var plexUser = new UserBuilder().Build();
            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).Returns(plexUser);
        }

        private void GivenNoMatchingUser()
        {
            _userService.GetUserFromPlexId(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenAMatchingUser(bool isDisabled)
        {
            _matchingDbUser = new UserRowBuilder().WithIsDisabled(isDisabled).Build();

            _userService.GetUserFromPlexId(Arg.Any<int>()).Returns(_matchingDbUser);
        }


        private void GivenATokenIsCreated()
        {
            _createdToken = Guid.NewGuid().ToString();

            _tokenService.CreateToken(Arg.Any<UserRow>()).Returns(_createdToken);
        }

        private void GivenARefreshTokenIsCreated()
        {
            _createdRefreshToken = new UserRefreshTokenRowBuilder().Build();

            _tokenService.CreateRefreshToken().Returns(_createdRefreshToken);
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenAnErrorIsThrown(string message, string description, HttpStatusCode httpStatusCode)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(message)
                          .Where(x => x.Description == description)
                          .Where(x => x.StatusCode == httpStatusCode);
        }

        private void ThenUserIsUpdatedCorrectly()
        {
            _commandAction.Should().NotThrow();

            _matchingDbUser.Should().NotBeNull();
            
            var now = DateTime.UtcNow;

            _matchingDbUser.LastLoginUtc.Should().NotBeNull();

            var loginDiff = (_matchingDbUser.LastLoginUtc.Value - now).Milliseconds;

            loginDiff.Should().BeLessOrEqualTo(500);
        }

        private async Task ThenATokenIsReturnedCorrectly()
        {
            var response = await _commandAction();

            response.Should().NotBeNull();
            response.AccessToken.Should().Be(_createdToken);
        }

        private async Task ThenARefreshTokenIsReturnedCorrectly()
        {
            var response = await _commandAction();

            response.Should().NotBeNull();
            response.RefreshToken.Should().Be(_createdRefreshToken.Token);
        }

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }
    }
}
