﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PlexRequests.Core.Settings;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Sync.SyncProcessors;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Sync.UnitTests.SyncProcessors
{
    public class MovieProcessorTests
    {
        private readonly MovieProcessor _underTest;

        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;

        private readonly Fixture _fixture;

        private PlexMediaContainer _plexMediaContainer;
        private Func<Task> _commandAction;
        private MediaItemResult _mediaItemResult;

        public MovieProcessorTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _plexService = Substitute.For<IPlexService>();
            _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            loggerFactory.CreateLogger<MovieProcessor>().Returns(Substitute.For<ILogger>());

            var plexSettings = _fixture.Create<PlexSettings>();

            _underTest = new MovieProcessor(_plexService, _mediaItemProcessor, plexSettings, loggerFactory);
        }

        [Fact]
        private void Gets_Local_Movie_MediaItems()
        {
            this.Given(x => x.GivenAContainerWithMetadata())
                .Given(x => x.GivenAProcessedMediaItem())
                .When(x => x.WhenAnActionIsCreated())
                .Then(x => x.ThenIsSuccessful())
                .Then(x => x.ThenLocalMediaItemsWereRetrieved())
                .BDDfy();
        }

        [Fact]
        private void A_Media_Item_Is_Processed()
        {
            this.Given(x => x.GivenAContainerWithMetadata())
                .Given(x => x.GivenAProcessedMediaItem())
                .When(x => x.WhenAnActionIsCreated())
                .Then(x => x.ThenIsSuccessful())
                .Then(x => x.ThenAMediaItemWasProcessed())
                .BDDfy();
        }

        [Fact]
        private void A_Result_Is_Updated()
        {
            this.Given(x => x.GivenAContainerWithMetadata())
                .Given(x => x.GivenAProcessedMediaItem())
                .When(x => x.WhenAnActionIsCreated())
                .Then(x => x.ThenIsSuccessful())
                .Then(x => x.ThenAResultWasUpdated())
                .BDDfy();
        }

        [Fact]
        private void Returns_Correct_SyncResult()
        {
            this.Given(x => x.GivenAContainerWithMetadata())
                .Given(x => x.GivenAProcessedMediaItem())
                .When(x => x.WhenAnActionIsCreated())
                .Then(x => x.ThenIsSuccessful())
                .BDDfy();
        }

        private void GivenAContainerWithMetadata()
        {
            _plexMediaContainer = _fixture.Create<PlexMediaContainer>();

            foreach (var metadata in _plexMediaContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = _fixture.Create<int>().ToString();
            }
        }

        private void GivenAProcessedMediaItem()
        {
            _mediaItemResult = _fixture.Create<MediaItemResult>();

            _mediaItemProcessor.GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(), Arg.Any<List<PlexMediaItemRow>>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(_mediaItemResult);
        }

        private void WhenAnActionIsCreated()
        {
            var fullRefresh = _fixture.Create<bool>();
            var authToken = _fixture.Create<string>();
            var plexUri = _fixture.Create<string>();
            var machineIdentifier = _fixture.Create<string>();

            _commandAction = async () =>
                await _underTest.Synchronise(_plexMediaContainer, fullRefresh, authToken, plexUri, machineIdentifier);
        }

        private void ThenIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenLocalMediaItemsWereRetrieved()
        {
            _plexService.Received().GetMediaItems(Arg.Is(PlexMediaTypes.Movie));
        }

        private void ThenAMediaItemWasProcessed()
        {
            _mediaItemProcessor.Received().GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(), Arg.Any<List<PlexMediaItemRow>>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void ThenAResultWasUpdated()
        {
            _mediaItemProcessor
                .Received().UpdateResult(Arg.Any<SyncResult>(), Arg.Any<bool>(), Arg.Any<PlexMediaItemRow>());
        }
    }
}