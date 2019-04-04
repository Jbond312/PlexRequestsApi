using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models.Auth;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Settings;
using PlexRequests.Store.Models;
using Shouldly;
using User = PlexRequests.Plex.Models.User;

namespace PlexRequests.UnitTests.Models.Auth
{
    [TestFixture]
    public class AddAdminCommandHandlerTests
    {
        private const string LocalIp = "192.168.0.1";

        private AddAdminCommandHandler _underTest;
        private IUserService _userService;
        private ITokenService _tokenService;
        private IPlexService _plexService;
        private IPlexApi _plexApi;
        private IOptions<PlexSettings> _plexSettings;
        private ILogger<AddAdminCommandHandler> _logger;

        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _tokenService = Substitute.For<ITokenService>();
            _plexService = Substitute.For<IPlexService>();
            _plexApi = Substitute.For<IPlexApi>();
            _logger = Substitute.For<ILogger<AddAdminCommandHandler>>();

            _fixture = new Fixture();

            var settings = _fixture.Create<PlexSettings>();
            _plexSettings = Options.Create(settings);

            _underTest = new AddAdminCommandHandler(_userService, _plexService, _tokenService, _plexApi, _plexSettings, _logger);
        }

        [Test]
        public async Task Throws_Error_When_Admin_Already_Created()
        {
            var command = _fixture.Create<AddAdminCommand>();

            _userService.IsAdminCreated().Returns(true);

            var exception = await Should.ThrowAsync<PlexRequestException>(() => _underTest.Handle(command, CancellationToken.None));

            exception.Message.ShouldBe("Unable to add Plex Admin", "An Admin account has already been created");
        }

        [Test]
        public async Task Creates_Admin_User()
        {
            var command = _fixture.Create<AddAdminCommand>();

            var plexSignInUser = MockPlexUserSignsIn();

            _userService.IsAdminCreated().Returns(false);
            
            await _underTest.Handle(command, CancellationToken.None);

            await _userService.Received().CreateUser(Arg.Is<Store.Models.User>(createdUser => IsCreatedUserValid(createdUser, plexSignInUser)));
        }

        [Test]
        public async Task Does_Not_Create_Server_If_Not_Owned()
        {
            var command = _fixture.Create<AddAdminCommand>();

            MockPlexUserSignsIn();
            MockPlexServers();

            _userService.IsAdminCreated().Returns(false);

            await _underTest.Handle(command, CancellationToken.None);

            await _plexService.DidNotReceive().Create(Arg.Any<PlexServer>());
        }

        [Test]
        public async Task Creates_Admin_Server()
        {
            var command = _fixture.Create<AddAdminCommand>();

            MockPlexUserSignsIn();
            var plexServers = MockPlexServers();
            plexServers[0].Owned = "1";

            var serverLibraries = MockLibraries();

            _userService.IsAdminCreated().Returns(false);

            await _underTest.Handle(command, CancellationToken.None);

            await _plexService.Received().Create(Arg.Is<PlexServer>(createdServer => IsCreatedServerValid(createdServer, plexServers, serverLibraries)));
        }

        [Test]
        public async Task Returns_Correct_Response()
        {
            var command = _fixture.Create<AddAdminCommand>();

            MockPlexUserSignsIn();

            Store.Models.User dbUser = null;
            await _userService.CreateUser(Arg.Do<Store.Models.User>(x => dbUser = x));
            
            _userService.IsAdminCreated().Returns(false);

            var token = _fixture.Create<string>();
            _tokenService.CreateToken(Arg.Any<Store.Models.User>()).Returns(token);

            var result = await _underTest.Handle(command, CancellationToken.None);

            result.ShouldNotBeNull();
            result.AccessToken.ShouldBe(token);

            dbUser.ShouldNotBeNull();

            _tokenService.Received().CreateToken(Arg.Is(dbUser));
        }

        private bool IsCreatedServerValid(PlexServer createdServer, List<Server> plexServers,
            PlexMediaContainer serverLibraries)
        {
            createdServer.ShouldNotBeNull();

            var ownedServer = plexServers.First(x => x.Owned == "1");

            createdServer.AccessToken = ownedServer.AccessToken;
            createdServer.Name = ownedServer.Name;
            createdServer.LocalIp = LocalIp;
            createdServer.LocalPort = _plexSettings.Value.DefaultLocalPort;
            createdServer.ExternalIp = ownedServer.Address;
            createdServer.ExternalPort = Convert.ToInt32(ownedServer.Port);
            createdServer.Scheme = ownedServer.Scheme;

            foreach (var library in createdServer.Libraries)
            {
                var matchingLibrary =
                    serverLibraries.MediaContainer.Directory.FirstOrDefault(x => x.Key == library.Key);

                matchingLibrary.ShouldNotBeNull();

                IsCreatedLibraryValid(library, matchingLibrary);
            }

            return true;
        }

        private static void IsCreatedLibraryValid(PlexServerLibrary library, Directory matchingLibrary)
        {
            library.Title.ShouldBe(matchingLibrary.Title);
            library.Type.ShouldBe(matchingLibrary.Type);
        }

        private static bool IsCreatedUserValid(Store.Models.User createdUser, User plexUser)
        {
            createdUser.ShouldNotBeNull();
            createdUser.Username.ShouldBe(plexUser.Username);
            createdUser.Email.ShouldBe(plexUser.Email);
            createdUser.IsAdmin.ShouldBeTrue();
            createdUser.Roles.SequenceEqual(new List<string> {PlexRequestRoles.Admin, PlexRequestRoles.User}).ShouldBeTrue();
            return true;
        }

        private User MockPlexUserSignsIn()
        {
            var plexSignInUser = _fixture.Create<User>();

            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).Returns(plexSignInUser);

            return plexSignInUser;
        }

        private List<Server> MockPlexServers()
        {
            var plexServers = _fixture.Build<Server>()
                .With(x => x.Owned, "0")
                .With(x => x.Port, _fixture.Create<int>().ToString)
                .With(x => x.LocalAddresses, $"{LocalIp},192.168.0.2,4,5")
                .CreateMany()
                .ToList();

            _plexApi.GetServers(Arg.Any<string>()).Returns(plexServers);

            return plexServers;
        }

        private PlexMediaContainer MockLibraries()
        {
            var mediaContainer = _fixture.Create<PlexMediaContainer>();

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(mediaContainer);

            return mediaContainer;
        }
    }
}
