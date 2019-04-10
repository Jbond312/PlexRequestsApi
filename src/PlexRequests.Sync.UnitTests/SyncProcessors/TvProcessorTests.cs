using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.Sync.SyncProcessors;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Sync.UnitTests.SyncProcessors
{
    public class TvProcessorTests
    {
        private readonly TvProcessor _underTest;

        private readonly IPlexApi _plexApi;
        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;
        private readonly IAgentGuidParser _agentGuidParser;

        private readonly Fixture _fixture;

        private List<PlexMediaItem> _localMediaItems;
        private PlexMediaContainer _plexLibraryContainer;
        private PlexMediaItem _rootPlexMediaItem;
        private PlexMediaItem _updatedMediaItem;

        private PlexMediaContainer _showContainer;
        private PlexMediaContainer _seasonMetadata;
        private (AgentTypes agentType, string agentId) _seasonAgentDetails;

        private Func<Task> _commandAction;

        private bool _fullRefresh = false;
        private bool _rootPlexMediaItemHasSeason = true;
        private int _seasonRatingKey;

        public TvProcessorTests()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _plexService = Substitute.For<IPlexService>();
            _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
            _agentGuidParser = Substitute.For<IAgentGuidParser>();

            _underTest = new TvProcessor(_plexApi, _plexService, _mediaItemProcessor, _agentGuidParser);

            _fixture = new Fixture();
        }

        [Fact]
        private void Local_Media_Items_Are_Retrieved()
        {
           this.Given(x => x.GivenLocalMediaItems())
                .Given(x => x.GivenALibraryContainer())
                .Given(x => x.GivenAProcessedMediaItem(_rootPlexMediaItemHasSeason))
                .Given(x => x.GivenNoChildMediaItems())
                .When(x => x.WhenAnActionIsCreated(_fullRefresh))
                .Then(x => x.ThenTheResponseIsSuccessful())
                .Then(x => x.ThenLocalMediaItemsWereReturned())
                .BDDfy();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        private void Does_Not_Process_Same_RatingKey_More_Than_Once(bool fullRefresh)
        {
            this.Given(x => x.GivenLocalMediaItems())
                .Given(x => x.GivenALibraryContainerWithOnlyOneUniqueKey())
                .Given(x => x.GivenAProcessedMediaItem(_rootPlexMediaItemHasSeason))
                .Given(x => x.GivenNoChildMediaItems())
                .When(x => x.WhenAnActionIsCreated(fullRefresh))
                .Then(x => x.ThenTheResponseIsSuccessful())
                .Then(x => x.ThenResultOnlyUpdatedOnce())
                .BDDfy();
        }

        [Fact]
        private void Processed_MediaItem_Is_Retrieved()
        {
            this.Given(x => x.GivenLocalMediaItems())
                .Given(x => x.GivenALibraryContainer())
                .Given(x => x.GivenAProcessedMediaItem(_rootPlexMediaItemHasSeason))
                .Given(x => x.GivenNoChildMediaItems())
                .When(x => x.WhenAnActionIsCreated(_fullRefresh))
                .Then(x => x.ThenTheResponseIsSuccessful())
                .Then(x => x.ThenProcessedMediaItemWasReturned())
                .BDDfy();
        }
        
        [Fact]
        private void When_Partial_Refresh_RatingKey_Is_GrandParent_RatingKey()
        {
            this.Given(x => x.GivenLocalMediaItems())
                .Given(x => x.GivenALibraryContainer())
                .Given(x => x.GivenAProcessedMediaItem(_rootPlexMediaItemHasSeason))
                .Given(x => x.GivenNoChildMediaItems())
                .When(x => x.WhenAnActionIsCreated(_fullRefresh))
                .Then(x => x.ThenTheResponseIsSuccessful())
                .Then(x => x.ThenGrandParentRatingKeyWasUsed())
                .BDDfy();
        }

        [Fact]
        private void Seasons_Are_Retrieved_From_Show_RatingKey()
        {
            _seasonRatingKey = _fixture.Create<int>();
            _rootPlexMediaItemHasSeason = false;
            _seasonAgentDetails = _fixture.Create<(AgentTypes, string)>();
            
            this.Given(x => x.GivenLocalMediaItems())
                .Given(x => x.GivenASingleLibraryMetadata())
                .Given(x => x.GivenAContainer(_seasonRatingKey, ref _showContainer))
                .Given(x => x.GivenMetadata(_seasonRatingKey, ref _seasonMetadata))
                .Given(x => x.GivenAgentDetailsForGuid(_seasonMetadata, _seasonAgentDetails))
                .Given(x => x.GivenNoEpisodeMetadata(_seasonRatingKey))
                .Given(x => x.GivenAProcessedMediaItem(_rootPlexMediaItemHasSeason))
                .Given(x => x.GivenAMediaItemIsUpdated())
                .When(x => x.WhenAnActionIsCreated(_fullRefresh))
                .Then(x => x.ThenTheResponseIsSuccessful())
                .Then(x => x.ThenTheSeasonIsCorrect())
                .BDDfy();
        }
        
        [Fact]
        private void SyncResult_Is_Updated()
        {
            this.Given(x => x.GivenLocalMediaItems())
                .Given(x => x.GivenALibraryContainer())
                .Given(x => x.GivenAProcessedMediaItem(_rootPlexMediaItemHasSeason))
                .Given(x => x.GivenNoChildMediaItems())
                .Given(x => x.GivenAMediaItemIsUpdated())
                .When(x => x.WhenAnActionIsCreated(_fullRefresh))
                .Then(x => x.ThenTheResponseIsSuccessful())
                .Then(x => x.ThenSyncResultWasUpdated())
                .BDDfy();
        }
        
        private void GivenLocalMediaItems()
        {
            _localMediaItems = _fixture.CreateMany<PlexMediaItem>().ToList();
            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(_localMediaItems);
        }

        private void GivenALibraryContainer()
        {
            _plexLibraryContainer = _fixture.Create<PlexMediaContainer>();

            foreach (var metadata in _plexLibraryContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = _fixture.Create<int>().ToString();
                metadata.GrandParentRatingKey = _fixture.Create<int>().ToString();
            }
        }
        
        private void GivenALibraryContainerWithOnlyOneUniqueKey()
        {
            _plexLibraryContainer = _fixture.Create<PlexMediaContainer>();

            var ratingKey = _fixture.Create<int>().ToString();
            
            foreach (var metadata in _plexLibraryContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = ratingKey;
                metadata.GrandParentRatingKey = ratingKey;
            }
        }

        private void GivenASingleLibraryMetadata()
        {
            _plexLibraryContainer = _fixture.Create<PlexMediaContainer>();
            _plexLibraryContainer.MediaContainer.Metadata = _fixture.Build<Metadata>()
                                                                    .With(x => x.RatingKey, _fixture.Create<int>().ToString)
                                                                    .With(x => x.GrandParentRatingKey, _fixture.Create<int>().ToString)
                                                                    .CreateMany(1)
                                                                    .ToList();
        }

        private void GivenAProcessedMediaItem(bool hasSeasons = true)
        {
            var isNewMediaItem = _fixture.Create<bool>();
            _rootPlexMediaItem = _fixture.Create<PlexMediaItem>();

            if (!hasSeasons)
            {
                _rootPlexMediaItem.Seasons = new List<PlexSeason>();
            }
            
            _mediaItemProcessor.GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(), Arg.Any<List<PlexMediaItem>>(),
                                   Arg.Any<string>(), Arg.Any<string>())
                               .Returns((isNewMediaItem, _rootPlexMediaItem));
        }

        private void GivenAMediaItemIsUpdated()
        {
            _mediaItemProcessor.UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Do<PlexMediaItem>(x => _updatedMediaItem = x));
        }

        private void GivenNoChildMediaItems()
        {
            _plexApi.GetChildrenMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).ReturnsNull();
        }
        
        private void GivenAContainer(int seasonRatingKey, ref PlexMediaContainer container)
        {
            if (container == null)
            {
                container = _fixture.Create<PlexMediaContainer>();
            }
            
            container.MediaContainer.Metadata = _fixture.Build<Metadata>()
                                                                    .With(x => x.RatingKey, seasonRatingKey.ToString)
                                                                    .CreateMany(1)
                                                                    .ToList();

            _plexApi.GetChildrenMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(container);
        }

        private void GivenMetadata(int seasonRatingKey, ref PlexMediaContainer metadata)
        {
            if (metadata == null)
            {
                metadata = _fixture.Create<PlexMediaContainer>();
            }

            _plexApi.GetMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Is(seasonRatingKey)).Returns(metadata);
        }

        private void GivenNoEpisodeMetadata(int seasonRatingKey)
        {
            _plexApi.GetChildrenMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Is(seasonRatingKey)).ReturnsNull();
        }

        private void GivenAgentDetailsForGuid(PlexMediaContainer mediaContainer, (AgentTypes, string) agentDetails)
        {
            var guid = mediaContainer.MediaContainer.Metadata.First().Guid;
            _agentGuidParser.TryGetAgentDetails(Arg.Is(guid)).Returns(agentDetails);
        }

        private void WhenAnActionIsCreated(bool fullRefresh)
        {
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();

            _commandAction = async () =>
                await _underTest.Synchronise(_plexLibraryContainer, fullRefresh, authToken, plexUri);
        }

        private void ThenTheResponseIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenLocalMediaItemsWereReturned()
        {
            _plexService.Received().GetMediaItems(Arg.Is(PlexMediaTypes.Show));
        }
        
        private void ThenGrandParentRatingKeyWasUsed()
        {
            var grandParentKeys =
                _plexLibraryContainer.MediaContainer.Metadata.Select(x => Convert.ToInt32(x.GrandParentRatingKey)).ToArray();
            foreach (var key in grandParentKeys)
            {
                _mediaItemProcessor.Received().GetMediaItem(Arg.Is(key), Arg.Any<PlexMediaTypes>(),
                    Arg.Any<List<PlexMediaItem>>(), Arg.Any<string>(), Arg.Any<string>());
            }
        }

        private void ThenResultOnlyUpdatedOnce()
        {
            _mediaItemProcessor.Received(1).UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItem>());
        }

        private void ThenProcessedMediaItemWasReturned()
        {
            _mediaItemProcessor.Received().GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(),
                Arg.Any<List<PlexMediaItem>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void ThenSyncResultWasUpdated()
        {
            _mediaItemProcessor.Received().UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItem>());
        }

        private void ThenTheSeasonIsCorrect()
        {
            _updatedMediaItem.Should().NotBeNull();
            _updatedMediaItem.Seasons.Should().NotBeNullOrEmpty();

            _updatedMediaItem.Seasons.Count.Should().Be(1);

            var seasonMetadata = _seasonMetadata.MediaContainer.Metadata.First();
            
            var updatedSeason = _updatedMediaItem.Seasons.First();
            updatedSeason.Key.Should().Be(_seasonRatingKey);
            updatedSeason.Title.Should().Be(seasonMetadata.Title);
            updatedSeason.Season.Should().Be(seasonMetadata.Index);
            updatedSeason.AgentType.Should().Be(_seasonAgentDetails.agentType);
            updatedSeason.AgentSourceId.Should().Be(_seasonAgentDetails.agentId);
            updatedSeason.Episodes.Should().BeEmpty();
        }
    }
}
