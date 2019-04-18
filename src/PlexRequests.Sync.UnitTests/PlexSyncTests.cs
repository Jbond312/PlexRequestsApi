using System;
using System.Collections.Generic;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using PlexRequests.Plex;
using PlexRequests.Repository.Models;
using System.Threading.Tasks;
using FluentAssertions;
using PlexRequests.Core.Services;
using PlexRequests.Core.Settings;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Enums;
using PlexRequests.Sync.SyncProcessors;
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

        private readonly Fixture _fixture;

        private PlexServer _plexServer;
        private Func<Task> _commandAction;
        private PlexMediaContainer _remoteLibraryContainer;
        private ISyncProcessor _syncProcessor;
        private SyncResult _syncProcessorResult;
        private List<PlexMediaItem> _createdMediaItems;
        private List<PlexMediaItem> _updatedMediaItems;

        public PlexSyncTests()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();
            _processorProvider = Substitute.For<IProcessorProvider>();
            _completionService = Substitute.For<ICompletionService>();
            
            var logger = Substitute.For<ILogger<PlexSync>>();

            _fixture = new Fixture();

            var plexSettings = _fixture.Create<PlexSettings>();
            var options = Options.Create(plexSettings);

            _underTest = new PlexSync(_plexApi, _plexService, _completionService, _processorProvider, options, logger);
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
                .BDDfy();
        }
        
        [Fact]
        private void When_New_And_Existing_Items_In_Sync_Result_They_Are_Persisted()
        {
            this.Given(x => x.GivenAPlexServer())
                .Given(x => x.GivenASingleEnabledLibrary())
                .Given(x => x.GivenMatchingRemoteLibraries())
                .Given(x => x.GivenASyncProcessor())
                .Given(x => x.GivenSyncResultPersisted())
                .When(x => x.WhenACommandActionIsCreated(true))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenSyncResultsShouldHaveBeenPersisted())
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
                .Given(x => x.GivenSyncResultPersisted())
                .When(x => x.WhenACommandActionIsCreated(true))
                .Then(x => x.ThenCommandShouldBeSuccessful())
                .Then(x => x.ThenRequestsAreAutoCompleted())
                .BDDfy();
        }
        
        private void GivenAPlexServer()
        {
            _plexServer = _fixture.Create<PlexServer>();

            _plexService.GetServer().Returns(_plexServer);
        }

        private void GivenAllServerLibrariesAreEnabled()
        {
            foreach (var library in _plexServer.Libraries)
            {
                library.IsEnabled = true;
            }
        }

        private void GivenASingleEnabledLibrary()
        {
            var plexLibrary = _fixture.Build<PlexServerLibrary>()
                                      .With(x => x.IsEnabled, true)
                                      .Create();

            _plexServer.Libraries = new List<PlexServerLibrary> {plexLibrary};
        }

        private void GivenMatchingRemoteLibraries()
        {
            _remoteLibraryContainer = _fixture.Create<PlexMediaContainer>();

            for (var i = 0; i < _plexServer.Libraries.Count; i++)
            {
                _remoteLibraryContainer.MediaContainer.Directory[i].Key = _plexServer.Libraries[i].Key;
            }
            
            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_remoteLibraryContainer);
        }

        private void GivenNoMatchingRemoteLibraries()
        {
            _remoteLibraryContainer = _fixture.Create<PlexMediaContainer>();

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(_remoteLibraryContainer);
        }

        private void GivenASyncProcessor()
        {
            _syncProcessor = Substitute.For<ISyncProcessor>();
            _syncProcessorResult = _fixture.Create<SyncResult>();

            _syncProcessor
                .Synchronise(Arg.Any<PlexMediaContainer>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(_syncProcessorResult);
            
            _processorProvider.GetProcessor(Arg.Any<string>()).Returns(_syncProcessor);
        }

        private void GivenSyncResultPersisted()
        {
            _plexService.CreateMany(Arg.Do<List<PlexMediaItem>>(x => _createdMediaItems = x));
            _plexService.UpdateMany(Arg.Do<List<PlexMediaItem>>(x => _updatedMediaItems = x));
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

        private void ThenSyncResultsShouldHaveBeenPersisted()
        {
            _createdMediaItems.Should().BeEquivalentTo(_syncProcessorResult.NewItems);
            _updatedMediaItems.Should().BeEquivalentTo(_syncProcessorResult.ExistingItems);
        }

        private void ThenNoSynchronisationShouldOccur()
        {
            _syncProcessor.DidNotReceive().Synchronise(Arg.Any<PlexMediaContainer>(), Arg.Any<bool>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void ThenRequestsAreAutoCompleted()
        {
            _completionService
                .Received(1).AutoCompleteRequests(Arg.Any<Dictionary<RequestAgent, PlexMediaItem>>(), Arg.Any<PlexMediaTypes>());
        }
    }
}
