using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core.Helpers;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Sync.SyncProcessors;
using PlexRequests.UnitTests.Builders;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using PlexRequests.UnitTests.Builders.Settings;
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

        private List<PlexMediaItemRow> _localMediaItems;
        private PlexMediaContainer _plexLibraryContainer;
        private PlexMediaItemRow _rootPlexMediaItem;
        private PlexMediaItemRow _updatedMediaItem;

        private PlexMediaContainer _showContainer;
        private PlexMediaContainer _seasonMetadata;
        private AgentGuidParserResult _seasonAgentDetails;

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
            var loggerFactory = Substitute.For<ILoggerFactory>();
            loggerFactory.CreateLogger<TvProcessor>().Returns(Substitute.For<ILogger>());

            var plexSettings = new PlexSettingsBuilder().Build();

            _underTest = new TvProcessor(_plexApi, _plexService, _mediaItemProcessor, plexSettings, _agentGuidParser, loggerFactory);
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
            _seasonRatingKey = new Random().Next(1, int.MaxValue);
            _rootPlexMediaItemHasSeason = false;
            _seasonAgentDetails = new AgentGuidParserResult(AgentTypes.TheMovieDb, Guid.NewGuid().ToString());

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
            _localMediaItems = new TvPlexMediaItemRowBuilder().CreateMany();
            _plexService.GetMediaItems(Arg.Any<PlexMediaTypes>()).Returns(_localMediaItems);
        }

        private void GivenALibraryContainer()
        {
            _plexLibraryContainer = new PlexMediaContainerBuilder().Build();

            foreach (var metadata in _plexLibraryContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = new Random().Next(1, int.MaxValue).ToString();
                metadata.GrandParentRatingKey = new Random().Next(1, int.MaxValue).ToString();
            }
        }

        private void GivenALibraryContainerWithOnlyOneUniqueKey()
        {
            _plexLibraryContainer = new PlexMediaContainerBuilder().Build();

            var ratingKey = new Random().Next(1, int.MaxValue).ToString();

            foreach (var metadata in _plexLibraryContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = ratingKey;
                metadata.GrandParentRatingKey = ratingKey;
            }
        }

        private void GivenASingleLibraryMetadata()
        {
            _plexLibraryContainer = new PlexMediaContainerBuilder().Build();

            _plexLibraryContainer.MediaContainer.Metadata = new MetadataBuilder().CreateMany(1);
        }

        private void GivenAProcessedMediaItem(bool hasSeasons = true)
        {
            var mediaItemResult = new MediaItemResult(true, new TvPlexMediaItemRowBuilder().Build());
            _rootPlexMediaItem = mediaItemResult.MediaItem;

            if (!hasSeasons)
            {
                _rootPlexMediaItem.PlexSeasons = new List<PlexSeasonRow>();
            }

            _mediaItemProcessor.GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(), Arg.Any<List<PlexMediaItemRow>>(),
                                   Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                               .Returns(mediaItemResult);
        }

        private void GivenAMediaItemIsUpdated()
        {
            _mediaItemProcessor.UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Do<PlexMediaItemRow>(x => _updatedMediaItem = x));
        }

        private void GivenNoChildMediaItems()
        {
            _plexApi.GetChildrenMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).ReturnsNull();
        }

        private void GivenAContainer(int seasonRatingKey, ref PlexMediaContainer container)
        {
            if (container == null)
            {
                container = new PlexMediaContainerBuilder().Build();
            }

            container.MediaContainer.Metadata = new MetadataBuilder().WithRatingKey(seasonRatingKey.ToString()).CreateMany(1);

            _plexApi.GetChildrenMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(container);
        }

        private void GivenMetadata(int seasonRatingKey, ref PlexMediaContainer metadata)
        {
            if (metadata == null)
            {
                metadata = new PlexMediaContainerBuilder().Build();
            }

            _plexApi.GetMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Is(seasonRatingKey)).Returns(metadata);
        }

        private void GivenNoEpisodeMetadata(int seasonRatingKey)
        {
            _plexApi.GetChildrenMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Is(seasonRatingKey)).ReturnsNull();
        }

        private void GivenAgentDetailsForGuid(PlexMediaContainer mediaContainer, AgentGuidParserResult agentDetails)
        {
            var guid = mediaContainer.MediaContainer.Metadata.First().Guid;
            _agentGuidParser.TryGetAgentDetails(Arg.Is(guid)).Returns(agentDetails);
        }

        private void WhenAnActionIsCreated(bool fullRefresh)
        {
            var authToken = Guid.NewGuid().ToString();
            var plexUri = Guid.NewGuid().ToString();
            var machineIdentifier = Guid.NewGuid().ToString();

            _commandAction = async () =>
                await _underTest.Synchronise(_plexLibraryContainer, fullRefresh, authToken, plexUri, machineIdentifier);
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
                    Arg.Any<List<PlexMediaItemRow>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            }
        }

        private void ThenResultOnlyUpdatedOnce()
        {
            _mediaItemProcessor.Received(1).UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItemRow>());
        }

        private void ThenProcessedMediaItemWasReturned()
        {
            _mediaItemProcessor.Received().GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(),
                Arg.Any<List<PlexMediaItemRow>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void ThenSyncResultWasUpdated()
        {
            _mediaItemProcessor.Received().UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItemRow>());
        }

        private void ThenTheSeasonIsCorrect()
        {
            _updatedMediaItem.Should().NotBeNull();
            _updatedMediaItem.PlexSeasons.Should().NotBeNullOrEmpty();

            _updatedMediaItem.PlexSeasons.Count.Should().Be(1);

            var seasonMetadata = _seasonMetadata.MediaContainer.Metadata.First();

            var updatedSeason = _updatedMediaItem.PlexSeasons.First();
            updatedSeason.MediaItemKey.Should().Be(_seasonRatingKey);
            updatedSeason.Title.Should().Be(seasonMetadata.Title);
            updatedSeason.Season.Should().Be(seasonMetadata.Index);
            updatedSeason.AgentType.Should().Be(_seasonAgentDetails.AgentType);
            updatedSeason.AgentSourceId.Should().Be(_seasonAgentDetails.AgentSourceId);
            updatedSeason.PlexEpisodes.Should().BeEmpty();
        }
    }
}
