using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.Sync.SyncProcessors;

namespace PlexRequests.Sync.UnitTests.SyncProcessors
{
    [TestFixture]
    public class MovieProcessorTests
    {
        private MovieProcessor _underTest;

        private IPlexService _plexService;
        private IMediaItemProcessor _mediaItemProcessor;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _plexService = Substitute.For<IPlexService>();
            _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();

            _underTest = new MovieProcessor(_plexService, _mediaItemProcessor);

            _fixture = new Fixture();
        }

        [Test]
        public async Task Gets_Local_MediaItems()
        {
            var request = _fixture.Create<SyncRequest>();

            request.LibraryContainer.MediaContainer.Metadata = new List<Metadata>();

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);

            await _plexService.Received().GetMediaItems(Arg.Is(PlexMediaTypes.Movie));
        }

        [Test]
        public async Task Gets_MediaItem_From_MediaItemProcessor()
        {
            var request = _fixture.Create<SyncRequest>();

            foreach (var metadata in request.LibraryContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = _fixture.Create<int>().ToString();
            }

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            MockGetMediaItem();

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);
        }

        [Test]
        public async Task UpdatesResult_After_Getting_MediaItem()
        {
            var request = _fixture.Create<SyncRequest>();

            foreach (var metadata in request.LibraryContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = _fixture.Create<int>().ToString();
            }

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            MockGetMediaItem();

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);

            var metadataCount = request.LibraryContainer.MediaContainer.Metadata.Count;

            _mediaItemProcessor.Received(metadataCount).UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItem>());
        }

        private void MockGetMediaItem()
        {
            var getMediaItemResponse = _fixture.Create<(bool, PlexMediaItem)>();

            _mediaItemProcessor.GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(), Arg.Any<List<PlexMediaItem>>(),
                Arg.Any<string>(), Arg.Any<string>()).Returns(getMediaItemResponse);
        }
    }
}
