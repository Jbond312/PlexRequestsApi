using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Sync.SyncProcessors;
using PlexRequests.UnitTests.Builders.DataAccess;
using PlexRequests.UnitTests.Builders.Plex;
using PlexRequests.UnitTests.Builders.Settings;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Sync.UnitTests.SyncProcessors
{
    public class MovieProcessorTests
    {
        private readonly MovieProcessor _underTest;

        private readonly IPlexService _plexService;
        private readonly IMediaItemProcessor _mediaItemProcessor;

        private PlexMediaContainer _plexMediaContainer;
        private Func<Task> _commandAction;
        private MediaItemResult _mediaItemResult;

        public MovieProcessorTests()
        {
            _plexService = Substitute.For<IPlexService>();
            _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            loggerFactory.CreateLogger<MovieProcessor>().Returns(Substitute.For<ILogger>());

            var plexSettings = new PlexSettingsBuilder().Build();

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
            _plexMediaContainer = new PlexMediaContainerBuilder().Build();

            foreach (var metadata in _plexMediaContainer.MediaContainer.Metadata)
            {
                metadata.RatingKey = new Random().Next(1, int.MaxValue).ToString();
            }
        }

        private void GivenAProcessedMediaItem()
        {
            _mediaItemResult = new MediaItemResult(true, new MoviePlexMediaItemRowBuilder().Build());

            _mediaItemProcessor.GetMediaItem(Arg.Any<int>(), Arg.Any<PlexMediaTypes>(), Arg.Any<List<PlexMediaItemRow>>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(_mediaItemResult);
        }

        private void WhenAnActionIsCreated()
        {
            const bool fullRefresh = true;
            var authToken = Guid.NewGuid().ToString();
            var plexUri = Guid.NewGuid().ToString();
            var machineIdentifier = Guid.NewGuid().ToString();

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