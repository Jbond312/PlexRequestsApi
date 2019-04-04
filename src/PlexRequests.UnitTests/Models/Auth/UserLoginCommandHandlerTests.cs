using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models.Auth;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using Shouldly;

namespace PlexRequests.UnitTests.Models.Auth
{
    [TestFixture]
    public class UserLoginCommandHandlerTests
    {
        private UserLoginCommandHandler _underTest;
        private IUserService _userService;
        private ITokenService _tokenService;
        private IPlexApi _plexApi;
        private ILogger<UserLoginCommandHandler> _logger;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _userService = Substitute.For<IUserService>();
            _tokenService = Substitute.For<ITokenService>();
            _plexApi = Substitute.For<IPlexApi>();
            _logger = Substitute.For<ILogger<UserLoginCommandHandler>>();

            _underTest = new UserLoginCommandHandler(_userService, _tokenService, _plexApi, _logger);

            _fixture = new Fixture();
        }

        [Test]
        public async Task Calls_Plex_SignIn_With_Credentials()
        {
            var command = _fixture.Create<UserLoginCommand>();

            MockPlexUserSignsIn();
            MockGetDbUserFromPlexId();
            
            await _underTest.Handle(command, CancellationToken.None);

            await _plexApi.Received().SignIn(command.Username, command.Password);
        }

        [Test]
        public async Task Throws_Error_When_Null_Plex_User()
        {
            var command = _fixture.Create<UserLoginCommand>();

            var exception = await Should.ThrowAsync<PlexRequestException>(() => _underTest.Handle(command, CancellationToken.None));

            exception.Message.ShouldBe("Unable to login to Plex with the given credentials");
        }

        [Test]
        public async Task Calls_UserService_GetUser_With_Plex_UserId()
        {
            var command = _fixture.Create<UserLoginCommand>();

            var plexSignInUser = MockPlexUserSignsIn();
            MockGetDbUserFromPlexId();

            await _underTest.Handle(command, CancellationToken.None);

            await _userService.Received().GetUserFromPlexId(plexSignInUser.Id);
        }

        [Test]
        public async Task Returns_Null_When_Disabled_Db_User()
        {
            var command = _fixture.Create<UserLoginCommand>();

            MockPlexUserSignsIn();
            var dbUser = MockGetDbUserFromPlexId();
            dbUser.IsDisabled = true;

            var result = await _underTest.Handle(command, CancellationToken.None);
            
            result.ShouldBeNull();
        }

        [Test]
        public async Task Returns_Null_When_Db_User_Not_Found()
        {
            var command = _fixture.Create<UserLoginCommand>();

            MockPlexUserSignsIn();

            var result = await _underTest.Handle(command, CancellationToken.None);

            result.ShouldBeNull();
        }

        [Test]
        public async Task Updates_LastLogin_Date()
        {
            var command = _fixture.Create<UserLoginCommand>();

            MockPlexUserSignsIn();
            var dbUser = MockGetDbUserFromPlexId();
            MockCreateToken();

            var now = DateTime.UtcNow;

            await _underTest.Handle(command, CancellationToken.None);

            await _userService.Received().UpdateUser(Arg.Is(dbUser));

            var loginDiff = (dbUser.LastLogin - now).Milliseconds;

            loginDiff.ShouldBeLessThanOrEqualTo(500);
        }

        [Test]
        public async Task Token_Created_From_TokenService()
        {
            var command = _fixture.Create<UserLoginCommand>();

            MockPlexUserSignsIn();
            var dbUser = MockGetDbUserFromPlexId();
            MockCreateToken();

            await _underTest.Handle(command, CancellationToken.None);

            _tokenService.Received().CreateToken(Arg.Is(dbUser));
        }

        [Test]
        public async Task Returns_Correct_Response()
        {
            var command = _fixture.Create<UserLoginCommand>();

            MockPlexUserSignsIn();
            MockGetDbUserFromPlexId();
            var token = MockCreateToken();

            var result = await _underTest.Handle(command, CancellationToken.None);

            result.ShouldNotBeNull();
            result.AccessToken.ShouldBe(token);
        }

        private User MockPlexUserSignsIn()
        {
            var plexSignInUser = _fixture.Create<User>();

            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).Returns(plexSignInUser);
            
            return plexSignInUser;
        }

        private Store.Models.User MockGetDbUserFromPlexId()
        {
            var user = _fixture.Build<Store.Models.User>()
                .With(x => x.IsDisabled, false)
                .Create();

            _userService.GetUserFromPlexId(Arg.Any<int>()).Returns(user);

            return user;
        }

        private string MockCreateToken()
        {
            var token = _fixture.Create<string>();
            _tokenService.CreateToken(Arg.Any<Store.Models.User>()).Returns(token);
            return token;
        }
    }
}
