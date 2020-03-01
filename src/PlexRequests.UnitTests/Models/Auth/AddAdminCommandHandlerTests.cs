using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Auth.Commands;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.UnitTests.Builders;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using PlexRequests.UnitTests.Builders.Settings;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlexApi _plexApi;
        private readonly IOptionsSnapshot<PlexSettings> _plexSettings;

        private AddAdminCommand _command;
        private Func<Task<ValidationContext<UserLoginCommandResult>>> _commandAction;
        private User _plexUser;
        private UserRow _createdAdminUser;
        private List<Server> _plexServers;
        private PlexServerRow _createdServer;
        private PlexMediaContainer _plexLibraryContainer;
        private string _createdToken;
        private UserRefreshTokenRow _createdRefreshToken;

        public AddAdminCommandHandlerTests()
        {
            _userService = Substitute.For<IUserService>();
            _tokenService = Substitute.For<ITokenService>();
            _plexService = Substitute.For<IPlexService>();
            _plexApi = Substitute.For<IPlexApi>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            var logger = Substitute.For<ILogger<AddAdminCommandHandler>>();

            var settings = new PlexSettingsBuilder().Build();
            _plexSettings = Substitute.For<IOptionsSnapshot<PlexSettings>>();
            _plexSettings.Value.Returns(settings);

            _underTest = new AddAdminCommandHandler(_userService, _plexService, _tokenService, _unitOfWork, _plexApi, _plexSettings, logger);
        }

        [Fact]
        private void Throws_Error_When_Admin_Already_Created()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(true))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Unable to add Plex Admin", "An Admin account has already been created"))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Invalid_Plex_Credentials()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenInvalidPlexCredentials())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid PlexCredentials", "The Login credentials for Plex were invalid"))
                .Then(x => x.ThenChangesAreNotCommitted())
                .BDDfy();
        }

        [Fact]
        private void An_Admin_User_Is_Created()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAnAdminIsCreated())
                .Given(x => x.GivenARefreshTokenIsReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnAdminUserWasCreated())
                .Then(x => x.ThenChangesAreCommitted())
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
                .Given(x => x.GivenARefreshTokenIsReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAServerWasCreated(false))
                .Then(x => x.ThenChangesAreCommitted())
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
                .Given(x => x.GivenARefreshTokenIsReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAServerWasCreated(true))
                .Then(x => x.ThenChangesAreCommitted())
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
                .Given(x => x.GivenARefreshTokenIsReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandReturnsAccessToken())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void Refresh_Token_Is_Returned_Successfully()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAnAdminAccount(false))
                .Given(x => x.GivenValidPlexCredentials())
                .Given(x => x.GivenAPlexServerWasFound())
                .Given(x => x.GivenARefreshTokenIsReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandReturnsRefreshToken())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = new AddAdminCommand
            {
                Username = "test@test.com",
                Password = "password"
            };
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
            _plexUser = new UserBuilder().Build();
            _plexApi.SignIn(Arg.Any<string>(), Arg.Any<string>()).Returns(_plexUser);
        }

        private void GivenAnAdminIsCreated()
        {
            _userService.AddUser(Arg.Do<UserRow>(x => _createdAdminUser = x));
        }

        private void GivenAPlexServerWasFound()
        {
            _plexServers = new ServerBuilder().CreateMany();

            var firstServer = _plexServers.First();
            firstServer.Owned = "1";
            firstServer.Port = new Random().Next(1, int.MaxValue).ToString();
            firstServer.LocalAddresses = $"{LocalIp},{LocalIp}1,{LocalIp}2";

            _plexApi.GetServers(Arg.Any<string>()).Returns(_plexServers);
        }

        private void GivenAServerIsCreated()
        {
            _plexService.AddServer(Arg.Do<PlexServerRow>(x => _createdServer = x));
        }

        private void GivenServerLibraries()
        {
            _plexLibraryContainer = new PlexMediaContainerBuilder().WithMetadata().Build();

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_plexLibraryContainer);
        }

        private void GivenATokenIsReturned()
        {
            _createdToken = Guid.NewGuid().ToString();
            
            _tokenService.CreateToken(Arg.Any<UserRow>()).Returns(_createdToken);
        }

        private void GivenARefreshTokenIsReturned()
        {
            _createdRefreshToken = new UserRefreshTokenRowBuilder().Build();

            _tokenService.CreateRefreshToken().Returns(_createdRefreshToken);
        }

        private async Task ThenAnErrorIsThrown(string message, string description)
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeFalse();
            var matchingError = result.ValidationErrors.FirstOrDefault(x => x.Message == message && x.Description == description);
            matchingError.Should().NotBeNull();
        }

        private void ThenAnAdminUserWasCreated()
        {
            _commandAction.Should().NotThrow();

            _createdAdminUser.Should().NotBeNull();

            _createdAdminUser.Should().NotBeNull();
            _createdAdminUser.Username.Should().Be(_plexUser.Username);
            _createdAdminUser.Email.Should().Be(_plexUser.Email);
            _createdAdminUser.IsAdmin.Should().BeTrue();
            var adminRoles = _createdAdminUser.UserRoles.Select(x => x.Role);
            adminRoles.Should()
                             .BeEquivalentTo(new List<string> { PlexRequestRoles.Admin, PlexRequestRoles.User, PlexRequestRoles.Commenter });
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
                var expectedLibraries = _plexLibraryContainer.MediaContainer.Directory.Select(x => new PlexLibraryRow
                {
                    LibraryKey = x.Key,
                    Title = x.Title,
                    Type = x.Type
                }).ToList();
                _createdServer.PlexLibraries.Should().BeEquivalentTo(expectedLibraries, options => options.Excluding(x => x.CreatedUtc));
            }
            else
            {
                _createdServer.PlexLibraries.Should().BeEmpty();
            }
        }

        private async Task ThenCommandReturnsAccessToken()
        {
            var response = await _commandAction();

            response.Should().NotBeNull();
            response.Data.AccessToken.Should().Be(_createdToken);
        }

        private async Task ThenCommandReturnsRefreshToken()
        {
            var response = await _commandAction();

            response.Should().NotBeNull();
            response.Data.RefreshToken.Should().Be(_createdRefreshToken.Token);
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
