using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using PlexRequests.Plex;
using System.Threading.Tasks;
using FluentAssertions;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex.Models;
using PlexRequests.Sync.SyncProcessors;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using PlexRequests.UnitTests.Builders.Settings;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Sync.UnitTests
{
    public class PlexSyncTests
    {
        private readonly PlexSync _underTest;

        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly IProcessorProvider _processorProvider;
        private readonly ICompletionService _completionService;
        private readonly IUnitOfWork _unitOfWork;

        private PlexServerRow _plexServer;
        private Func<Task> _commandAction;
        private PlexMediaContainer _remoteLibraryContainer;
        private ISyncProcessor _syncProcessor;
        private SyncResult _syncProcessorResult = new SyncResult();

        public PlexSyncTests()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();
            _processorProvider = Substitute.For<IProcessorProvider>();
            _completionService = Substitute.For<ICompletionService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            var logger = Substitute.For<ILogger<PlexSync>>();

            var plexSettings = new PlexSettingsBuilder().Build();
            var options = Substitute.For<IOptionsSnapshot<PlexSettings>>();
            options.Value.Returns(plexSettings);

            _underTest = new PlexSync(_plexApi, _plexService, _completionService, _processorProvider, _unitOfWork, options, logger);
        }

        [Fact]
        private void A_Full_Refresh_Should_Remove_All_Existing_Plex_Content()
        {
            this.Given(x => x.GivenAPlexServer())
                .Given(x => x.GivenAllServerLibrariesAreEnabled())
                .Given(x => x.GivenNoMatchingRemoteLibraries())
                .When(x => x.WhenACommandActionIsCreated(true))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenExistingPlexItemsAreDeleted())
                .Then(x => x.ThenChangesAreNotCommited())
                .BDDfy();
        }

        [Fact]
        private void A_Full_Refresh_Should_Gather_All_Plex_Library_Metadata()
        {
            this.Given(x => x.GivenAPlexServer())
                .Given(x => x.GivenASingleEnabledLibrary())
                .Given(x => x.GivenMatchingRemoteLibraries())
                .Given(x => x.GivenASyncProcessor())
                .When(x => x.WhenACommandActionIsCreated(true))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenAllLibraryMetadataIsRetrieved())
                .Then(x => x.ThenChangesAreCommited())
                .BDDfy();
        }

        [Fact]
        private void A_Partial_Refresh_Should_Gather_Only_Recently_Added_Plex_Library_Metadata()
        {
            this.Given(x => x.GivenAPlexServer())
                .Given(x => x.GivenASingleEnabledLibrary())
                .Given(x => x.GivenMatchingRemoteLibraries())
                .Given(x => x.GivenASyncProcessor())
                .When(x => x.WhenACommandActionIsCreated(false))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenRecentlyAddedMetadataIsRetrieved())
                .Then(x => x.ThenChangesAreCommited())
                .BDDfy();
        }

        [Fact]
        private void When_New_And_Existing_Items_In_Sync_Result_They_Are_Persisted()
        {
            this.Given(x => x.GivenAPlexServer())
                .Given(x => x.GivenASingleEnabledLibrary())
                .Given(x => x.GivenMatchingRemoteLibraries())
                .Given(x => x.GivenASyncProcessor())
                .When(x => x.WhenACommandActionIsCreated(true))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenChangesAreCommited())
                .BDDfy();
        }

        [Fact]
        private void If_Local_Enabled_Library_Not_In_Remote_It_Should_Not_Sync()
        {
            this.Given(x => x.GivenAPlexServer())
                .Given(x => x.GivenASingleEnabledLibrary())
                .Given(x => x.GivenNoMatchingRemoteLibraries())
                .Given(x => x.GivenASyncProcessor())
                .When(x => x.WhenACommandActionIsCreated(true))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenNoSynchronisationShouldOccur())
                .BDDfy();
        }

        [Fact]
        private void Requests_Are_Auto_Completed_After_Sync()
        {
            this.Given(x => x.GivenAPlexServer())
                .Given(x => x.GivenASingleEnabledLibrary())
                .Given(x => x.GivenMatchingRemoteLibraries())
                .Given(x => x.GivenASyncProcessor())
                .When(x => x.WhenACommandActionIsCreated(true))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenRequestsAreAutoCompleted())
                .Then(x => x.ThenChangesAreCommited())
                .BDDfy();
        }

        private void GivenAPlexServer()
        {
            _plexServer = new PlexServerRowBuilder().Build();

            _plexService.GetServer().Returns(_plexServer);
        }

        private void GivenAllServerLibrariesAreEnabled()
        {
            foreach (var library in _plexServer.PlexLibraries)
            {
                library.IsEnabled = true;
            }
        }

        private void GivenASingleEnabledLibrary()
        {
            var plexLibrary = new PlexLibraryRowBuilder().WithIsEnabled(true).Build();
            _plexServer.PlexLibraries = new List<PlexLibraryRow> { plexLibrary };
        }

        private void GivenMatchingRemoteLibraries()
        {
            _remoteLibraryContainer = new PlexMediaContainerBuilder().Build();

            for (var i = 0; i < _plexServer.PlexLibraries.Count; i++)
            {
                _remoteLibraryContainer.MediaContainer.Directory[i].Key = _plexServer.PlexLibraries.ElementAt(i).LibraryKey;
            }

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_remoteLibraryContainer);
        }

        private void GivenNoMatchingRemoteLibraries()
        {
            _remoteLibraryContainer = new PlexMediaContainerBuilder().Build();

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_remoteLibraryContainer);
        }

        private void GivenASyncProcessor()
        {
            _syncProcessor = Substitute.For<ISyncProcessor>();
            _syncProcessor
                .Synchronise(Arg.Any<PlexMediaContainer>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(_syncProcessorResult);

            _processorProvider.GetProcessor(Arg.Any<string>()).Returns(_syncProcessor);
        }

        private void WhenACommandActionIsCreated(bool fullRefresh)
        {
            _commandAction = async () => await _underTest.Synchronise(fullRefresh);
        }

        private void ThenCommandShouldBeSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenExistingPlexItemsAreDeleted()
        {
            _plexService.Received().DeleteAllMediaItems();
        }

        private void ThenAllLibraryMetadataIsRetrieved()
        {
            _plexApi.Received().GetLibrary(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void ThenRecentlyAddedMetadataIsRetrieved()
        {
            _plexApi.Received().GetRecentlyAdded(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void ThenNoSynchronisationShouldOccur()
        {
            _syncProcessor.DidNotReceive().Synchronise(Arg.Any<PlexMediaContainer>(), Arg.Any<bool>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void ThenRequestsAreAutoCompleted()
        {
            _completionService
                .Received(1).AutoCompleteRequests(Arg.Any<Dictionary<MediaAgent, PlexMediaItemRow>>(), Arg.Any<PlexMediaTypes>());
        }

        private void ThenChangesAreCommited()
        {
            _unitOfWork.Received().CommitAsync();
        }

        private void ThenChangesAreNotCommited()
        {
            _unitOfWork.DidNotReceive().CommitAsync();
        }
    }
}
