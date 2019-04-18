using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.ApiRequests.Auth.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Auth
{
    public class UserLoginCommandHandlerTests
    {
        private readonly UserLoginCommandHandler _underTest;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IPlexApi _plexApi;

        private readonly Fixture _fixture;
        private UserLoginCommand _command;
        private Repository.Models.User _matchingDbUser;
        private Repository.Models.User _updatedUser;
        private string _createdToken;
        private Func<Task<UserLoginCommandResult>> _commandAction;

        public UserLoginCommandHandlerTests()
        {
            _userService = Substitute.For<IUserService>();
            _tokenService = Substitute.For<ITokenService>();
            _plexApi = Substitute.For<IPlexApi>();
            var logger = Substitute.For<ILogger<UserLoginCommandHandler>>();

            _underTest = new UserLoginCommandHandler(_userService, _tokenService, _plexApi, logger);

            _fixture = new Fixture();
        }

        [Fact]
        private void Throws_Error_When_Invalid_PlexCredentials()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenInvalidPlexCredentials())
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
                .Given(x => x.GivenAUserIsUpdated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenUserIsUpdatedCorrectly())
                .BDDfy();
        }

        [Fact]
        private void Token_Is_Created_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAMatchingUser(false))
                .Given(x => x.GivenATokenIsCreated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenATokenIsReturnedCorrectly())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<UserLoginCommand>();
        }

        private void GivenInvalidPlexCredentials()
        {
            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).ReturnsNull();
        }

        private void GivenValidPlexCredentials()
        {
            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).Returns(_fixture.Create<User>());
        }

        private void GivenNoMatchingUser()
        {
            _userService.GetUserFromPlexId(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenAMatchingUser(bool isDisabled)
        {
            _matchingDbUser = _fixture.Build<Repository.Models.User>()
                                      .With(x => x.IsDisabled, isDisabled)
                                      .Create();

            _userService.GetUserFromPlexId(Arg.Any<int>()).Returns(_matchingDbUser);
        }

        private void GivenAUserIsUpdated()
        {
            _userService.UpdateUser(Arg.Do<Repository.Models.User>(x => _updatedUser = x));
        }

        private void GivenATokenIsCreated()
        {
            _createdToken = _fixture.Create<string>();

            _tokenService.CreateToken(Arg.Any<Repository.Models.User>()).Returns(_createdToken);
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

            _updatedUser.Should().NotBeNull();

            _updatedUser.Should().Be(_matchingDbUser);

            var now = DateTime.UtcNow;

            var loginDiff = (_updatedUser.LastLogin - now).Milliseconds;

            loginDiff.Should().BeLessOrEqualTo(500);
        }

        private async Task ThenATokenIsReturnedCorrectly()
        {
            var response = await _commandAction();

            response.Should().NotBeNull();
            response.AccessToken.Should().Be(_createdToken);
        }
    }
}
