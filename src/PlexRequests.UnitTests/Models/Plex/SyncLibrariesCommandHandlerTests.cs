using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using NSubstitute;
using PlexRequests.ApiRequests.Plex.Commands;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using PlexRequests.UnitTests.Builders.Settings;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class SyncLibrariesCommandHandlerTests
    {
        private readonly IRequestHandler<SyncLibrariesCommand> _underTest;

        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;
        
        private SyncLibrariesCommand _command;
        private Func<Task> _commandAction;
        private PlexMediaContainer _remoteLibraryContainer;
        private PlexServerRow _plexServer;

        public SyncLibrariesCommandHandlerTests()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            var plexSettings = new PlexSettingsBuilder().Build();
            var options = Substitute.For<IOptionsSnapshot<PlexSettings>>();
            options.Value.Returns(plexSettings);

            _underTest = new SyncLibrariesCommandHandler(_plexApi, _plexService, _unitOfWork, options);
        }

        [Fact]
        private void Libraries_Not_On_Remote_But_On_Local_Are_Set_To_Archived_And_Disabled()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAServer())
                .Given(x => x.GivenRemoteLibraries(false))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAllLocalLibrariesArchived())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void Libraries_On_Remote_But_Not_On_Local_Are_Added_To_Local_Libraries()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAServer())
                .Given(x => x.GivenNoLocalLibraries())
                .Given(x => x.GivenRemoteLibraries(true))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAllRemoteLibrariesCreatedLocally())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = new SyncLibrariesCommand();
        }

        private void GivenAServer()
        {
            _plexServer = new PlexServerRowBuilder().Build();
            _plexService.GetServer().Returns(_plexServer);
        }

        private void GivenRemoteLibraries(bool hasRemoteLibraries)
        {
            _remoteLibraryContainer = new PlexMediaContainerBuilder().WithMetadata().WithDirectories().Build();

            if (!hasRemoteLibraries)
            {
                _remoteLibraryContainer.MediaContainer.Directory = new List<Directory>();
            }

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_remoteLibraryContainer);
        }

        private void GivenNoLocalLibraries()
        {
            _plexServer.PlexLibraries = new List<PlexLibraryRow>();
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenAllLocalLibrariesArchived()
        {
            _commandAction.Should().NotThrow();

            _plexServer.Should().NotBeNull();
            _plexServer.Should().BeEquivalentTo(_plexServer, options => options.Excluding(x => x.PlexLibraries));

            foreach (var library in _plexServer.PlexLibraries)
            {
                library.IsArchived.Should().BeTrue();
                library.IsEnabled.Should().BeFalse();
            }
        }

        private void ThenAllRemoteLibrariesCreatedLocally()
        {
            _commandAction.Should().NotThrow();

            _plexServer.Should().NotBeNull();
            _plexServer.Should().BeEquivalentTo(_plexServer, options => options.Excluding(x => x.PlexLibraries).Excluding(x => x.CreatedUtc));

            var expectedLibraries = _remoteLibraryContainer.MediaContainer.Directory.Select(x => new PlexLibraryRow
            {
                LibraryKey = x.Key,
                Title = x.Title,
                Type = x.Type,
                IsEnabled = false
            }).ToList();

            _plexServer.PlexLibraries.Should().BeEquivalentTo(expectedLibraries, options => options.Excluding(x => x.CreatedUtc));
        }

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }
    }
}