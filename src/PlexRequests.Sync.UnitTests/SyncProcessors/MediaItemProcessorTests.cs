using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Helpers;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.Sync.SyncProcessors;
using Shouldly;

namespace PlexRequests.Sync.UnitTests.SyncProcessors
{
    [TestFixture]
    public class MediaItemProcessorTests
    {
        private const string ValidAgentGuid = "com.plexapp.agents.thetvdb://73141/15/7?lang=en";
        private const AgentTypes ValidAgentGuidType = AgentTypes.TheTvDb;
        private const string ValidAgentGuidId = "73141";

        private MediaItemProcessor _underTest;

        private IPlexApi _plexApi;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _plexApi = Substitute.For<IPlexApi>();

            _underTest = new MediaItemProcessor(_plexApi);

            _fixture = new Fixture();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Updates_Correct_Items_If_New_Item(bool isNew)
        {
            var result = _fixture.Create<SyncResult>();
            var mediaItem = _fixture.Create<PlexMediaItem>();

            _underTest.UpdateResult(result, isNew, mediaItem);

            if (isNew)
            {
                result.NewItems.ShouldContain(mediaItem);
            }
            else
            {
                result.ExistingItems.ShouldContain(mediaItem);
            }
        }

        [Test]
        [TestCase("com.plexapp.agents.thetvdb://73141/15/7?lang=en", AgentTypes.TheTvDb, "73141")]
        [TestCase("com.plexapp.agents.thetvdb://73141/15?lang=en", AgentTypes.TheTvDb, "73141")]
        [TestCase("com.plexapp.agents.thetvdb://73141?lang=en", AgentTypes.TheTvDb, "73141")]
        [TestCase("com.plexapp.agents.imdb://tt1727824?lang=en", AgentTypes.Imdb, "tt1727824")]
        [TestCase("com.plexapp.agents.themoviedb://446021?lang=en", AgentTypes.TheMovieDb, "446021")]
        public void Returns_Correct_Agent_Details(string agentGuid, AgentTypes expectedAgentType, string expectedAgentId)
        {
            var (agentType, agentSourceId) = _underTest.GetAgentDetails(agentGuid);

            agentType.ShouldBe(expectedAgentType);
            agentSourceId.ShouldBe(expectedAgentId);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Throws_Error_When_No_AgentGuid_Specified(string agentGuid)
        {
            var exception = Should.Throw<PlexRequestException>(() => _underTest.GetAgentDetails(agentGuid));

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("PlexMetadataGuid");
            exception.Description.ShouldBe("The PlexMetadataGuid should not be null or empty");
            exception.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Test]
        [TestCase("abcd")]
        [TestCase("a.b.c.d")]
        [TestCase("a.b.c.d:1234")]
        [TestCase("a.b.c.d:/1234")]
        [TestCase("a.b.c.d://1234")]
        [TestCase("://1234")]
        [TestCase("//1234")]
        [TestCase("/1234")]
        [TestCase("com.plexapp.agents.imdb:3141")]
        [TestCase("com.plexapp.agents.imdb:/3141")]
        [TestCase("com.plexapp.agents.imdb://3141")]
        [TestCase("...themoviedb://446021?lang=en")]
        public void Throws_Error_When_Invalid_AgentGuid_Specified(string agentGuid)
        {
            var exception = Should.Throw<PlexRequestException>(() => _underTest.GetAgentDetails(agentGuid));

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("PlexMetadataGuid");
            exception.Description.ShouldBe("The PlexMetadataGuid was not in the expected format");
            exception.LoggableObject.ShouldBe(agentGuid);
            exception.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Test]
        [TestCase("com.plexapp.agents.foo://73141/15/7?lang=en")]
        public void Throws_Error_When_Invalid_AgentType_Found(string agentGuid)
        {
            var exception = Should.Throw<PlexRequestException>(() => _underTest.GetAgentDetails(agentGuid));

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("PlexMetadataGuid");
            exception.Description.ShouldBe("No AgentType could be extracted from the agent guid");
            exception.LoggableObject.ShouldBe(agentGuid);
            exception.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task Gets_Metadata_For_RatingKey()
        {
            var ratingKey = _fixture.Create<int>();
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();

            MockGetMetadata();

            await _underTest.GetMediaItem(ratingKey, PlexMediaTypes.Movie, new List<PlexMediaItem>(), authToken, plexUri);

            await _plexApi.Received(1).GetMetadata(Arg.Is(authToken), Arg.Is(plexUri), Arg.Is(ratingKey));
        }

        [Test]
        public async Task Sets_NewFlag_If_New_MediaItem()
        {
            var ratingKey = _fixture.Create<int>();
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();
            var localMedia = _fixture.CreateMany<PlexMediaItem>().ToList();

            MockGetMetadata();
            
            var (isNew, _) = await _underTest.GetMediaItem(ratingKey, PlexMediaTypes.Movie, localMedia, authToken, plexUri);

            isNew.ShouldBeTrue();
        }

        [Test]
        public async Task Sets_NewFlag_False_If_Exists()
        {
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();
            var localMedia = _fixture.CreateMany<PlexMediaItem>().ToList();

            var remoteMetadata = MockGetMetadata();

            var key = localMedia[0].Key;
            remoteMetadata.MediaContainer.Metadata[0].Key = key.ToString();

            var (isNew, _) = await _underTest.GetMediaItem(key, PlexMediaTypes.Movie, localMedia, authToken, plexUri);

            isNew.ShouldBeFalse();
        }

        [Test]
        public async Task VideoResolution_Correct_When_No_Media()
        {
            var ratingKey = _fixture.Create<int>();
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();
            var localMedia = _fixture.CreateMany<PlexMediaItem>().ToList();
            const PlexMediaTypes mediaType = PlexMediaTypes.Movie;

            var remoteMetadata = MockGetMetadata();
            remoteMetadata.MediaContainer.Metadata.First().Media = null;

            var (_, mediaItem) = await _underTest.GetMediaItem(ratingKey, mediaType, localMedia, authToken, plexUri);

            mediaItem.Resolution.ShouldBeNull();
        }

        [Test]
        public async Task Throws_Error_When_No_Metadata_Found_On_Container()
        {
            var ratingKey = _fixture.Create<int>();
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();
            var localMedia = _fixture.CreateMany<PlexMediaItem>().ToList();
            const PlexMediaTypes mediaType = PlexMediaTypes.Movie;

            var remoteMetadata = MockGetMetadata();
            remoteMetadata.MediaContainer.Metadata = null;

            var exception = await Should.ThrowAsync<PlexRequestException>(() =>
                _underTest.GetMediaItem(ratingKey, mediaType, localMedia, authToken, plexUri));

            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("PlexMediaContainer");
            exception.Description.ShouldBe($"No metadata was found for container with key: {ratingKey}");
        }

        [Test]
        public async Task MediaItem_Is_Correct_New_Item()
        {
            var ratingKey = _fixture.Create<int>();
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();
            var localMedia = _fixture.CreateMany<PlexMediaItem>().ToList();
            const PlexMediaTypes mediaType = PlexMediaTypes.Movie;

            var remoteMetadata = MockGetMetadata();
            
            var (_, mediaItem) = await _underTest.GetMediaItem(ratingKey, mediaType, localMedia, authToken, plexUri);

            var metadata = remoteMetadata.MediaContainer.Metadata.First();
            var media = metadata.Media.First();

            mediaItem.ShouldNotBeNull();
            mediaItem.Key.ShouldBe(ratingKey);
            mediaItem.MediaType.ShouldBe(mediaType);
            mediaItem.Title.ShouldBe(metadata.Title);
            mediaItem.Year.ShouldBe(metadata.Year);
            media.VideoResolution = media.VideoResolution;
            mediaItem.AgentType.ShouldBe(ValidAgentGuidType);
            mediaItem.AgentSourceId.ShouldBe(ValidAgentGuidId);
        }

        private PlexMediaContainer MockGetMetadata()
        {
            var plexMediaContainer = _fixture.Create<PlexMediaContainer>();

            _plexApi.GetMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(plexMediaContainer);

            plexMediaContainer.MediaContainer.Metadata.First().Guid = ValidAgentGuid;

            return plexMediaContainer;
        }
    }
}
