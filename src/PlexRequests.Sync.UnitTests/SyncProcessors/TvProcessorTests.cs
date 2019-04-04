using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TvProcessorTests
    {
        private TvProcessor _underTest;

        private IPlexApi _plexApi;
        private IPlexService _plexService;
        private IMediaItemProcessor _mediaItemProcessor;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();
            _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();

            _underTest = new TvProcessor(_plexApi, _plexService, _mediaItemProcessor);

            _fixture = new Fixture();
        }

        [Test]
        public async Task Gets_Local_MediaItems()
        {
            var request = CreateSyncRequest();

            request.LibraryContainer.MediaContainer.Metadata = new List<Metadata>();

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);

            await _plexService.Received().GetMediaItems(Arg.Is(PlexMediaTypes.Show));
        }

        [Test]
        public async Task Gets_MediaItem_From_MediaItemProcessor()
        {
            var request = CreateSyncRequest();

            CorrectMetadataKeys(request.LibraryContainer);

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            MockGetMediaItem();

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);
        }

        [Test]
        public async Task UpdatesResult_After_Getting_MediaItem()
        {
            var request = CreateSyncRequest();

            CorrectMetadataKeys(request.LibraryContainer);

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            MockGetMediaItem();

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);

            var metadataCount = request.LibraryContainer.MediaContainer.Metadata.Count;

            _mediaItemProcessor.Received(metadataCount).UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItem>());
        }

        [Test]
        public async Task Does_Not_Process_Same_Key_Twice()
        {
            var request = CreateSyncRequest();
            var keyId = _fixture.Create<int>().ToString();

            foreach (var metadata in request.LibraryContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = keyId;
                metadata.GrandParentRatingKey = keyId;
            }

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            MockGetMediaItem();

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);

            _mediaItemProcessor.Received(1).UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItem>());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Uses_Grandparent_RatingKey_If_Not_FullRefresh(bool fullRefresh)
        {
            var request = CreateSyncRequest();
            request.FullRefresh = fullRefresh;
            var firstMetadata = request.LibraryContainer.MediaContainer.Metadata.First();
            request.LibraryContainer.MediaContainer.Metadata = new List<Metadata>{firstMetadata};

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            MockGetMediaItem();
            CorrectMetadataKeys(request.LibraryContainer);

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);

            var ratingKey = Convert.ToInt32(firstMetadata.RatingKey);
            if (!fullRefresh)
            {
                ratingKey = Convert.ToInt32(firstMetadata.GrandParentRatingKey);
            }

            await _mediaItemProcessor.GetMediaItem(Arg.Is(ratingKey), Arg.Any<PlexMediaTypes>(),
                Arg.Any<List<PlexMediaItem>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public async Task Gets_Season_Metadata_For_Show()
        {
            var request = CreateSyncRequest();
            request.FullRefresh = true;

            CorrectMetadataKeys(request.LibraryContainer);

            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(new List<PlexMediaItem>());

            var mediaItem = MockGetMediaItem();
            var seasonMetadata = MockSeasonMetadata();
            seasonMetadata.MediaContainer.Metadata = new List<Metadata>();

            await _underTest.Synchronise(request.LibraryContainer, request.FullRefresh, request.AuthToken, request.PlexUri);
            
            await _plexApi.Received(1).GetChildrenMetadata(Arg.Is(request.AuthToken), Arg.Is(request.PlexUri),
                Arg.Is(mediaItem.Key));
        }

        private PlexMediaItem MockGetMediaItem()
        {
            var getMediaItemResponse = _fixture.Create<(bool, PlexMediaItem)>();

            _mediaItemProcessor.GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(), Arg.Any<List<PlexMediaItem>>(),
                Arg.Any<string>(), Arg.Any<string>()).Returns(getMediaItemResponse);

            return getMediaItemResponse.Item2;
        }

        private PlexMediaContainer MockSeasonMetadata()
        {
            var plexContainer = _fixture.Create<PlexMediaContainer>();

            CorrectMetadataKeys(plexContainer);

            _plexApi.GetChildrenMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(plexContainer);

            return plexContainer;
        }


        private SyncRequest CreateSyncRequest()
        {
            var request = _fixture.Create<SyncRequest>();
            var firstMetadata = request.LibraryContainer.MediaContainer.Metadata.First();
            request.LibraryContainer.MediaContainer.Metadata = new List<Metadata> { firstMetadata };
            return request;
        }
        private void CorrectMetadataKeys(PlexMediaContainer mediaContainer)
        {
            foreach (var metadata in mediaContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = _fixture.Create<int>().ToString();
                metadata.GrandParentRatingKey = _fixture.Create<int>().ToString();
            }
        }
    }
}
