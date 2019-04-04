using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Plex;
using PlexRequests.Settings;
using PlexRequests.Store.Models;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Plex.Models;
using PlexRequests.Sync.SyncProcessors;

namespace PlexRequests.Sync.UnitTests
{
    [TestFixture]
    public class PlexSyncTests
    {
        private PlexSync _underTest;

        private IPlexApi _plexApi;
        private IPlexService _plexService;
        private IProcessorProvider _processorProvider;
        private PlexSettings _plexSettings;
        private ILogger<PlexSync> _logger;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();
            _processorProvider = Substitute.For<IProcessorProvider>();
            _logger = Substitute.For<ILogger<PlexSync>>();

            _fixture = new Fixture();

            _plexSettings = _fixture.Create<PlexSettings>();
            var options = Options.Create(_plexSettings);

            _underTest = new PlexSync(_plexApi, _plexService, _processorProvider, options, _logger);
        }

        [Test]
        public async Task Gets_Plex_Server()
        {
            MockPlexServer();
            MockPlexLibraries();

            await _underTest.Synchronise(_fixture.Create<bool>());

            await _plexService.Received().GetServer();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Deletes_All_MediaItems_When_Refresh_Applicable(bool fullRefresh)
        {
            MockPlexServer();
            MockPlexLibraries();

            await _underTest.Synchronise(fullRefresh);

            if (fullRefresh)
            {
                await _plexService.Received().DeleteAllMediaItems();
            }
            else
            {
                await _plexService.DidNotReceive().DeleteAllMediaItems();
            }
        }

        [Test]
        public async Task Gets_Remote_Libraries()
        {
            var plexServer = MockPlexServer();
            var plexUri = plexServer.GetPlexUri(_plexSettings.ConnectLocally);
            MockPlexLibraries();

            await _underTest.Synchronise(_fixture.Create<bool>());

            await _plexApi.Received().GetLibraries(Arg.Is(plexServer.AccessToken), Arg.Is(plexUri));
        }

        [Test]
        public async Task Process_Not_Called_When_No_Matching_Remote_Libraries()
        {
            MockPlexServer();
            MockPlexLibraries();

            await _underTest.Synchronise(_fixture.Create<bool>());

            _processorProvider.DidNotReceive().GetProcessor(Arg.Any<string>());
        }

        [Test]
        public async Task Processor_Called_When_Valid_Library()
        {
            var plexServer = MockPlexServer();
            var remoteLibraries = MockPlexLibraries();

            plexServer.Libraries[0].Key = remoteLibraries.MediaContainer.Directory[0].Key;

            _processorProvider.GetProcessor(Arg.Any<string>()).ReturnsNull();

            await _underTest.Synchronise(_fixture.Create<bool>());

            _processorProvider.Received().GetProcessor(Arg.Is(plexServer.Libraries[0].Type));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Gets_Recently_Added_When_FullRefresh_False(bool fullRefresh)
        {
            var plexServer = MockPlexServer();
            var plexUri = plexServer.GetPlexUri(_plexSettings.ConnectLocally);
            var remoteLibraries = MockPlexLibraries();

            plexServer.Libraries[0].Key = remoteLibraries.MediaContainer.Directory[0].Key;

            MockProcessorSynchronise();

            await _underTest.Synchronise(fullRefresh);

            if (fullRefresh)
            {
                await _plexApi.Received().GetLibrary(Arg.Is(plexServer.AccessToken), Arg.Is(plexUri),
                    Arg.Is(plexServer.Libraries[0].Key));
            }
            else
            {
                await _plexApi.Received().GetRecentlyAdded(Arg.Is(plexServer.AccessToken), Arg.Is(plexUri),
                    Arg.Is(plexServer.Libraries[0].Key));
            }
        }

        [Test]
        public async Task Calls_Processor_Synchronise()
        {
            var plexServer = MockPlexServer();
            var remoteLibraries = MockPlexLibraries();

            plexServer.Libraries[0].Key = remoteLibraries.MediaContainer.Directory[0].Key;

            var (_, processor) = MockProcessorSynchronise();

            await _underTest.Synchronise(_fixture.Create<bool>());

            await processor.Received(1).Synchronise(Arg.Any<PlexMediaContainer>(), Arg.Any<bool>(), Arg.Any<string>(),
                Arg.Any<string>());
        }

        [Test]
        public async Task Updates_MediaItems_From_SyncResult()
        {
            var plexServer = MockPlexServer();
            var remoteLibraries = MockPlexLibraries();

            plexServer.Libraries[0].Key = remoteLibraries.MediaContainer.Directory[0].Key;

            var (processorResult, _) = MockProcessorSynchronise();

            await _underTest.Synchronise(_fixture.Create<bool>());

            await _plexService.Received(1).CreateMany(Arg.Is(processorResult.NewItems));
            await _plexService.Received(1).UpdateMany(Arg.Is(processorResult.ExistingItems));
        }

        [Test]
        public async Task Did_Not_Sync_If_No_Enabled_Libraries()
        {
            var plexServer = MockPlexServer();
            var remoteLibraries = MockPlexLibraries();

            foreach (var library in plexServer.Libraries)
            {
                library.IsEnabled = false;
            }

            plexServer.Libraries[0].Key = remoteLibraries.MediaContainer.Directory[0].Key;

            await _underTest.Synchronise(_fixture.Create<bool>());

            _processorProvider.DidNotReceive().GetProcessor(Arg.Any<string>());
        }

        private (SyncResult, ISyncProcessor) MockProcessorSynchronise()
        {
            var processor = Substitute.For<ISyncProcessor>();

            var syncResult = _fixture.Create<SyncResult>();

            processor.Synchronise(Arg.Any<PlexMediaContainer>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(syncResult);

            _processorProvider.GetProcessor(Arg.Any<string>()).Returns(processor);

            return (syncResult, processor);
        }

        private PlexServer MockPlexServer()
        {
            var plexLibraries = _fixture
                .Build<PlexServerLibrary>()
                .With(x => x.IsEnabled, true)
                .CreateMany().ToList();

            var plexServer = _fixture
                .Build<PlexServer>()
                .With(x => x.Libraries, plexLibraries)
                .Create();

            _plexService.GetServer().Returns(plexServer);

            return plexServer;
        }

        private PlexMediaContainer MockPlexLibraries()
        {
            var container = _fixture.Create<PlexMediaContainer>();

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(container);

            return container;
        }
    }
}
