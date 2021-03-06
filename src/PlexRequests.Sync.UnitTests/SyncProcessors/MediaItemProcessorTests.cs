﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PlexRequests.Core.Helpers;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Sync.SyncProcessors;
using PlexRequests.UnitTests.Builders;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Sync.UnitTests.SyncProcessors
{
    public class MediaItemProcessorTests
    {
        private readonly MediaItemProcessor _underTest;

        private readonly IPlexApi _plexApi;

        private readonly IAgentGuidParser _agentGuidParser;

        private Func<Task<MediaItemResult>> _getMediaItemAction;
        private Action _updateResultAction;
        private PlexMediaContainer _plexMetadataContainer;
        private int _ratingKey;
        private AgentGuidParserResult _agentDetails;
        private PlexMediaTypes _mediaType;
        private List<PlexMediaItemRow> _localMedia;
        private SyncResult _syncResult;
        private PlexMediaItemRow _plexMediaItem;

        public MediaItemProcessorTests()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _agentGuidParser = Substitute.For<IAgentGuidParser>();

            var logger = Substitute.For<ILogger<MediaItemProcessor>>();
            _underTest = new MediaItemProcessor(_plexApi, _agentGuidParser, logger);
        }

        [Fact]
        private void Returns_Null_When_GetMediaItem_No_MediaContainer_Found()
        {
            const bool metadataExists = false;
            const bool isExistingMediaItem = false;

            this.Given(x => x.GivenARatingKey())
                .Given(x => x.GivenPlexMetadata(metadataExists))
                .When(x => x.WhenGetMediaItemActionIsCreated(isExistingMediaItem))
                .Then(x => x.ThenNullResult())
                .BDDfy();
        }

        [Fact]
        private void GetMediaItem_Returns_Correct_MediaItem_When_New_MediaItem()
        {
            const bool metadataExists = true;
            const bool isExistingMediaItem = false;

            this.Given(x => x.GivenARatingKey())
                .Given(x => x.GivenPlexMetadata(metadataExists))
                .Given(x => x.GivenAgentDetails())
                .When(x => x.WhenGetMediaItemActionIsCreated(isExistingMediaItem))
                .Then(x => x.ThenMediaItemIsCorrect(!isExistingMediaItem))
                .BDDfy();
        }

        [Fact]
        private void GetMediaItem_Returns_Correct_MediaItem_When_Existing_MediaItem()
        {
            const bool metadataExists = true;
            const bool isExistingMediaItem = true;

            this.Given(x => x.GivenARatingKey())
                .Given(x => x.GivenPlexMetadata(metadataExists))
                .Given(x => x.GivenAgentDetails())
                .When(x => x.WhenGetMediaItemActionIsCreated(isExistingMediaItem))
                .Then(x => x.ThenMediaItemIsCorrect(!isExistingMediaItem))
                .BDDfy();
        }

        [Fact]
        private void Update_Result_Adds_To_New_When_IsNew()
        {
            const bool isNew = true;
            this.Given(x => x.GivenAPlexMediaItem())
                .When(x => x.WhenUpdateResultActionIsCreated(isNew))
                .Then(x => x.ThenUpdateResultItemsAreAddedCorrectly(isNew))
                .BDDfy();
        }

        [Fact]
        private void Update_Result_Adds_To_Existing_When_Not_IsNew()
        {
            const bool isNew = false;
            this.Given(x => x.GivenAPlexMediaItem())
                .When(x => x.WhenUpdateResultActionIsCreated(isNew))
                .Then(x => x.ThenUpdateResultItemsAreAddedCorrectly(isNew))
                .BDDfy();
        }

        private void GivenPlexMetadata(bool metadataExists)
        {
            _plexMetadataContainer = new PlexMediaContainerBuilder().Build();

            if (!metadataExists)
            {
                _plexMetadataContainer.MediaContainer.Metadata = null;
            }

            _plexApi.GetMetadata(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(_plexMetadataContainer);
        }

        private void GivenARatingKey()
        {
            _ratingKey = new Random().Next(1, int.MaxValue);
        }

        private void GivenAgentDetails()
        {
            const AgentTypes agentType = AgentTypes.Imdb;
            var agentSourceId = Guid.NewGuid().ToString();

            _agentDetails = new AgentGuidParserResult(agentType, agentSourceId);

            _agentGuidParser.TryGetAgentDetails(Arg.Any<string>())
                            .Returns(_agentDetails);
        }

        private void GivenAPlexMediaItem()
        {
            _plexMediaItem = new MoviePlexMediaItemRowBuilder().Build();
        }

        private void WhenGetMediaItemActionIsCreated(bool isExistingMediaItem)
        {
            _mediaType = PlexMediaTypes.Show;
            _localMedia = new MoviePlexMediaItemRowBuilder().CreateMany();
            var authToken = Guid.NewGuid().ToString();
            var plexUri = Guid.NewGuid().ToString();
            var machineIdentifier = Guid.NewGuid().ToString();
            var plexUriFormat = Guid.NewGuid().ToString();

            if (isExistingMediaItem)
            {
                var matchingMediaItem = _localMedia.First();
                matchingMediaItem.MediaItemKey = _ratingKey;
                matchingMediaItem.MediaType = _mediaType;
            }

            _getMediaItemAction = async () => await _underTest.GetMediaItem(_ratingKey, _mediaType, _localMedia, authToken, plexUri, machineIdentifier, plexUriFormat);
        }

        private void WhenUpdateResultActionIsCreated(bool isNew)
        {
            _syncResult = new SyncResult();

            _updateResultAction = () => _underTest.UpdateResult(_syncResult, isNew, _plexMediaItem);

        }

        private async Task ThenMediaItemIsCorrect(bool isNewMediaItem)
        {
            var result = await _getMediaItemAction();

            result.IsNew.Should().Be(isNewMediaItem);

            var metadata = _plexMetadataContainer.MediaContainer.Metadata.First();

            result.MediaItem.Should().NotBeNull();
            result.MediaItem.MediaItemKey.Should().Be(_ratingKey);
            result.MediaItem.MediaType.Should().Be(_mediaType);
            result.MediaItem.Resolution.Should().Be(metadata.Media?.FirstOrDefault()?.VideoResolution);
            result.MediaItem.AgentType.Should().Be(_agentDetails.AgentType);
            result.MediaItem.AgentSourceId.Should().Be(_agentDetails.AgentSourceId);

            if (isNewMediaItem)
            {
                result.MediaItem.Title.Should().Be(metadata.Title);
                metadata.Year.Should().Be(metadata.Year);
            }
            else
            {
                var localMediaItem = _localMedia.First(x => x.MediaItemKey == _ratingKey);

                result.MediaItem.Title.Should().Be(localMediaItem.Title);
                result.MediaItem.Year.Should().Be(localMediaItem.Year);
            }
        }

        private void ThenUpdateResultItemsAreAddedCorrectly(bool isNew)
        {
            _updateResultAction.Should().NotThrow();

            _syncResult.Should().NotBeNull();

            if (isNew)
            {
                _syncResult.NewItems.Should().Contain(_plexMediaItem);
                _syncResult.ExistingItems.Should().NotContain(_plexMediaItem);
            }
            else
            {
                _syncResult.NewItems.Should().NotContain(_plexMediaItem);
                _syncResult.ExistingItems.Should().Contain(_plexMediaItem);
            }
        }

        private async Task ThenNullResult()
        {
            var result = await _getMediaItemAction();

            result.Should().BeNull();
        }
    }
}
