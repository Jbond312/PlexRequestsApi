using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.Models.Auth;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;
using User = PlexRequests.Plex.Models.User;

namespace PlexRequests.UnitTests.Models.Auth
{
    public class AddAdminCommandHandlerTests
    {
        private const string LocalIp = "192.168.0.1";

        private readonly AddAdminCommandHandler _underTest;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IPlexService _plexService;
        private readonly IPlexApi _plexApi;
        private readonly IOptions<PlexSettings> _plexSettings;

        private readonly Fixture _fixture;
        
        private AddAdminCommand _command;
        private Func<Task<UserLoginCommandResult>> _commandAction;
        private User _plexUser;
        private Repository.Models.User _createdAdminUser;
        private List<Server> _plexServers;
        private PlexServer _createdServer;
        private PlexMediaContainer _plexLibraryContainer;
        private string _createdToken;

        public AddAdminCommandHandlerTests()
        {
            _userService = Substitute.For<IUserService>();
            _tokenService = Substitute.For<ITokenService>();
            _plexService = Substitute.For<IPlexService>();
            _plexApi = Substitute.For<IPlexApi>();
            var logger = Substitute.For<ILogger<AddAdminCommandHandler>>();

            _fixture = new Fixture();

            var settings = _fixture.Create<PlexSettings>();
            _plexSettings = Options.Create(settings);

            _underTest = new AddAdminCommandHandler(_userService, _plexService, _tokenService, _plexApi, _plexSettings, logger);
        }

        [Fact]
        private void Throws_Error_When_Admin_Already_Created()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(true))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Unable to add Plex Admin",
                    "An Admin account has already been created", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Invalid_Plex_Credentials()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenInvalidPlexCredentials())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid PlexCredentials",
                    "The Login credentials for Plex were invalid.", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void An_Admin_User_Is_Created()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAnAdminIsCreated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnAdminUserWasCreated())
                .BDDfy();
        }
        
        [Fact]
        private void An_Admin_Server_Is_Created_With_No_Libraries()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAPlexServerWasFound())
                .Given(x => x.GivenAServerIsCreated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAServerWasCreated(false))
                .BDDfy();
        }
        
        [Fact]
        private void An_Admin_Server_Is_Created_With_Libraries()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAPlexServerWasFound())
                .Given(x => x.GivenAServerIsCreated())
                .Given(x => x.GivenServerLibraries())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAServerWasCreated(true))
                .BDDfy();
        }
        
        [Fact]
        private void Access_Token_Is_Returned_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAPlexServerWasFound())
                .Given(x => x.GivenATokenIsReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandReturnsAccessToken())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<AddAdminCommand>();
        }

        private void GivenAnAdminAccount(bool alreadyExists)
        {
            _userService.IsAdminCreated().Returns(alreadyExists);
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void GivenInvalidPlexCredentials()
        {
            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).ReturnsNull();
        }
        
        private void GivenValidPlexCredentials()
        {
            _plexUser = _fixture.Create<User>();
            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).Returns(_plexUser);
        }

        private void GivenAnAdminIsCreated()
        {
            _userService.CreateUser(Arg.Do<Repository.Models.User>(x => _createdAdminUser = x));
        }

        private void GivenAPlexServerWasFound()
        {
            _plexServers = _fixture.CreateMany<Server>().ToList();

            var firstServer = _plexServers.First();
            firstServer.Owned = "1";
            firstServer.Port = _fixture.Create<int>().ToString();
            firstServer.LocalAddresses = $"{LocalIp},{LocalIp}1,{LocalIp}2";

            _plexApi.GetServers(Arg.Any<string>()).Returns(_plexServers);
        }
        
        private void GivenAServerIsCreated()
        {
            _plexService.Create(Arg.Do<PlexServer>(x => _createdServer = x));
        }

        private void GivenServerLibraries()
        {
            _plexLibraryContainer = _fixture.Create<PlexMediaContainer>();

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_plexLibraryContainer);
        }

        private void GivenATokenIsReturned()
        {
            _createdToken = _fixture.Create<string>();
            
            _tokenService.CreateToken(Arg.Any<Repository.Models.User>()).Returns(_createdToken);
        }
        
        private void ThenAnErrorIsThrown(string message, string description, HttpStatusCode httpStatusCode)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(message)
                          .Where(x => x.Description == description)
                          .Where(x => x.StatusCode == httpStatusCode);
        }

        private void ThenAnAdminUserWasCreated()
        {
            _commandAction.Should().NotThrow();

            _createdAdminUser.Should().NotBeNull();
            
            _createdAdminUser.Should().NotBeNull();
            _createdAdminUser.Username.Should().Be(_plexUser.Username);
            _createdAdminUser.Email.Should().Be(_plexUser.Email);
            _createdAdminUser.IsAdmin.Should().BeTrue();
            _createdAdminUser.Roles.Should()
                             .BeEquivalentTo(new List<string> {PlexRequestRoles.Admin, PlexRequestRoles.User});
        }

        private void ThenAServerWasCreated(bool createdLibraries)
        {
            _commandAction.Should().NotThrow();

            _createdServer.Should().NotBeNull();
            var ownedServer = _plexServers.First(x => x.Owned == "1");

            _createdServer.AccessToken.Should().Be(ownedServer.AccessToken);
            _createdServer.Name.Should().Be(ownedServer.Name);
            _createdServer.LocalIp.Should().Be(LocalIp);
            _createdServer.LocalPort.Should().Be(_plexSettings.Value.DefaultLocalPort);
            _createdServer.ExternalIp.Should().Be(ownedServer.Address);
            _createdServer.ExternalPort.Should().Be(Convert.ToInt32(ownedServer.Port));
            _createdServer.Scheme.Should().Be(ownedServer.Scheme);

            if (createdLibraries)
            {
                var expectedLibraries = _plexLibraryContainer.MediaContainer.Directory.Select(x => new PlexServerLibrary
                {
                    Key = x.Key,
                    Title = x.Title,
                    Type = x.Type
                }).ToList();
                _createdServer.Libraries.Should().BeEquivalentTo(expectedLibraries);
            }
            else
            {
                _createdServer.Libraries.Should().BeEmpty();
            }
        }
        
        private async Task ThenCommandReturnsAccessToken()
        {
            var response = await _commandAction();

            response.Should().NotBeNull();
            response.AccessToken.Should().Be(_createdToken);
        }
    }
}
