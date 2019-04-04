using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Mapping;
using PlexRequests.Models.Plex;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Settings;
using PlexRequests.Store.Models;
using Shouldly;

namespace PlexRequests.UnitTests.Models.Plex
{
    [TestFixture]
    public class SyncLibrariesCommandHandlerTests
    {
        private SyncLibrariesCommandHandler _underTest;

        private IMapper _mapper;
        private IPlexApi _plexApi;
        private IPlexService _plexService;
        private PlexSettings _plexSettings;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new PlexProfile()); });
            _mapper = mapperConfig.CreateMapper();

            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();

            _fixture = new Fixture();

            _plexSettings = _fixture.Create<PlexSettings>();
            var options = Options.Create(_plexSettings);

            _underTest = new SyncLibrariesCommandHandler(_mapper, _plexApi, _plexService, options);
        }

        [Test]
        public async Task Gets_Server()
        {
            var command = _fixture.Create<SyncLibrariesCommand>();

            MockPlexServer();
            MockPlexLibraries();

            await _underTest.Handle(command, CancellationToken.None);

            await _plexService.Received().GetServer();
        }

        [Test]
        public async Task Gets_Libraries_From_Server()
        {
            var command = _fixture.Create<SyncLibrariesCommand>();

            var plexServer = MockPlexServer();
            MockPlexLibraries();

            await _underTest.Handle(command, CancellationToken.None);

            var plexUri = plexServer.GetPlexUri(_plexSettings.ConnectLocally);
            await _plexApi.Received().GetLibraries(Arg.Is(plexServer.AccessToken), Arg.Is(plexUri));
        }

        [Test]
        public async Task Archives_Deleted_Libraries()
        {
            var command = _fixture.Create<SyncLibrariesCommand>();

            var plexServer = MockPlexServer();
            var remoteLibraries = MockPlexLibraries();

            SyncLibraryKeys(plexServer, remoteLibraries);

            remoteLibraries.MediaContainer.Directory.RemoveAt(0);

            var result = await _underTest.Handle(command, CancellationToken.None);

            result.Libraries.Count(x => x.IsArchived).ShouldBe(1);
        }

        [Test]
        public async Task Adds_New_Libraries()
        {
            var command = _fixture.Create<SyncLibrariesCommand>();

            var plexServer = MockPlexServer();
            var remoteLibraries = MockPlexLibraries();

            SyncLibraryKeys(plexServer, remoteLibraries);

            plexServer.Libraries.RemoveAt(0);

            var serverCount = plexServer.Libraries.Count;

            var result = await _underTest.Handle(command, CancellationToken.None);

            result.Libraries.Count.ShouldBe(serverCount + 1);
        }

        [Test]
        public async Task Updates_Plex_Server()
        {
            var command = _fixture.Create<SyncLibrariesCommand>();

            var plexServer = MockPlexServer();
            MockPlexLibraries();

            await _underTest.Handle(command, CancellationToken.None);

            await _plexService.Received().Update(Arg.Is(plexServer));
        }

        private PlexServer MockPlexServer()
        {
            var libraries = _fixture.Build<PlexServerLibrary>()
                .With(x => x.IsArchived, false)
                .CreateMany().ToList();

            var plexServer = _fixture.Build<PlexServer>()
                .With(x => x.Libraries, libraries)
                .Create();

            _plexService.GetServer().Returns(plexServer);

            return plexServer;
        }

        private PlexMediaContainer MockPlexLibraries()
        {
            var libraryContainer = _fixture.Create<PlexMediaContainer>();

            _plexApi.GetLibraries(Arg.Any<string>(), Arg.Any<string>()).Returns(libraryContainer);

            return libraryContainer;
        }

        private static void SyncLibraryKeys(PlexServer plexServer, PlexMediaContainer remoteLibraries)
        {
            for (var i = 0; i < plexServer.Libraries.Count; i++)
            {
                var serverLibrary = plexServer.Libraries[i];
                var remoteLibrary = remoteLibraries.MediaContainer.Directory[i];
                remoteLibrary.Key = serverLibrary.Key;
            }
        }
    }
}
