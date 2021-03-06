using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Plex.Queries;
using PlexRequests.Functions.Mapping;
using PlexRequests.Plex;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class GetManyPlexServerLibraryQueryHandlerTests
    {
        private readonly GetManyPlexServerLibraryQueryHandler _underTest;
        private readonly IPlexService _plexService;

        private GetManyPlexServerLibraryQuery _request;
        private Func<Task<ValidationContext<GetManyPlexServerLibraryQueryResult>>> _commandAction;
        private PlexServerRow _server;

        public GetManyPlexServerLibraryQueryHandlerTests()
        {
            _plexService = Substitute.For<IPlexService>();
            
            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new PlexProfile()); });
            var mapper = mapperConfig.CreateMapper();
            
            _underTest = new GetManyPlexServerLibraryQueryHandler(mapper, _plexService);
        }

        [Fact]
        private void Retrieves_ServerLibraries_From_Repository()
        {
            this.Given(x => x.GivenAQueryRequest())
                .Given(x => x.GivenLibrariesAreReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenResponseIsSuccessful())
                .Then(x => x.ThenLibrariesWereRetrieved())
                .BDDfy();
        }
        
        [Fact]
        private void Returns_Correct_Libraries()
        {
            this.Given(x => x.GivenAQueryRequest())
                .Given(x => x.GivenLibrariesAreReturned())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenLibrariesAreCorrect())
                .BDDfy();
        }

        private void GivenAQueryRequest()
        {
            _request = new GetManyPlexServerLibraryQuery();
        }

        private void GivenLibrariesAreReturned()
        {
            _server = new PlexServerRowBuilder()
                .WithLibraries(3)
                .Build();
            _plexService.GetServer().Returns(_server);
        }
        
        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_request, CancellationToken.None);
        }

        private void ThenResponseIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenLibrariesWereRetrieved()
        {
            _plexService.Received().GetServer();
        }

        private async Task ThenLibrariesAreCorrect()
        {
            var result = await _commandAction();

            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();

            foreach (var actualLibrary in result.Data.Libraries)
            {
                var matchingLibrary = _server.PlexLibraries.FirstOrDefault(x => x.LibraryKey == actualLibrary.Key);

                matchingLibrary.Should().NotBeNull();
                // ReSharper disable once PossibleNullReferenceException
                actualLibrary.IsArchived.Should().Be(matchingLibrary.IsArchived);
                actualLibrary.IsEnabled.Should().Be(matchingLibrary.IsEnabled);
                actualLibrary.Title.Should().Be(matchingLibrary.Title);
                actualLibrary.Type.Should().Be(matchingLibrary.Type);
            }
        }
    }
}