using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using NSubstitute;
using PlexRequests.Core.Settings;
using PlexRequests.Models.Plex;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class SyncLibrariesCommandHandlerTests
    {
        private readonly IRequestHandler<SyncLibrariesCommand> _underTest;

        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;

        private readonly Fixture _fixture;

        private SyncLibrariesCommand _command;
        private PlexServer _updatedServer;
        private Func<Task> _commandAction;
        private PlexMediaContainer _remoteLibraryContainer;
        private PlexServer _plexServer;

        public SyncLibrariesCommandHandlerTests()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();

            _fixture = new Fixture();

            var plexSettings = _fixture.Create<PlexSettings>();
            var options = Options.Create(plexSettings);

            _underTest = new SyncLibrariesCommandHandler(_plexApi, _plexService, options);
        }

        [Fact]
        private void Libraries_Not_On_Remote_But_On_Local_Are_Set_To_Archived_And_Disabled()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAServer())
                .Given(x => x.GivenRemoteLibraries(false))
                .Given(x => x.GivenAServerIsUpdated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAllLocalLibrariesArchived())
                .BDDfy();
        }

        [Fact]
        private void Libraries_On_Remote_But_Not_On_Local_Are_Added_To_Local_Libraries()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAServer())
                .Given(x => x.GivenNoLocalLibraries())
                .Given(x => x.GivenRemoteLibraries(true))
                .Given(x => x.GivenAServerIsUpdated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAllRemoteLibrariesCreatedLocally())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<SyncLibrariesCommand>();
        }

        private void GivenAServer()
        {
            _plexServer = _fixture.Create<PlexServer>();
            _plexService.GetServer().Returns(_plexServer);
        }

        private void GivenRemoteLibraries(bool hasRemoteLibraries)
        {
            _remoteLibraryContainer = _fixture.Create<PlexMediaContainer>();

            if (!hasRemoteLibraries)
            {
                _remoteLibraryContainer.MediaContainer.Directory = new List<Directory>();
            }

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_remoteLibraryContainer);
        }

        private void GivenNoLocalLibraries()
        {
            _plexServer.Libraries = new List<PlexServerLibrary>();
        }

        private void GivenAServerIsUpdated()
        {
            _plexService.Update(Arg.Do<PlexServer>(x => _updatedServer = x));
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenAllLocalLibrariesArchived()
        {
            _commandAction.Should().NotThrow();

            _updatedServer.Should().NotBeNull();
            _updatedServer.Should().BeEquivalentTo(_plexServer, options => options.Excluding(x => x.Libraries));

            foreach (var library in _updatedServer.Libraries)
            {
                library.IsArchived.Should().BeTrue();
                library.IsEnabled.Should().BeFalse();
            }
        }

        private void ThenAllRemoteLibrariesCreatedLocally()
        {
            _commandAction.Should().NotThrow();

            _updatedServer.Should().NotBeNull();
            _updatedServer.Should().BeEquivalentTo(_plexServer, options => options.Excluding(x => x.Libraries));

            var expectedLibraries = _remoteLibraryContainer.MediaContainer.Directory.Select(x => new PlexServerLibrary
            {
                Key = x.Key,
                Title = x.Title,
                Type = x.Type,
                IsEnabled = false
            }).ToList();

            _updatedServer.Libraries.Should().BeEquivalentTo(expectedLibraries);
        }
    }
}