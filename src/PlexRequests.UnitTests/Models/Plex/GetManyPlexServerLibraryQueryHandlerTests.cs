using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Mapping;
using PlexRequests.Models.Plex;
using PlexRequests.Plex;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class GetManyPlexServerLibraryQueryHandlerTests
    {
        private readonly GetManyPlexServerLibraryQueryHandler _underTest;
        private readonly IPlexService _plexService;

        private readonly Fixture _fixture;
        
        private GetManyPlexServerLibraryQuery _request;
        private Func<Task<GetManyPlexServerLibraryQueryResult>> _commandAction;
        private PlexServer _server;

        public GetManyPlexServerLibraryQueryHandlerTests()
        {
            _fixture = new Fixture();

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
            _request = _fixture.Create<GetManyPlexServerLibraryQuery>();
        }

        private void GivenLibrariesAreReturned()
        {
            _server = _fixture.Create<PlexServer>();
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
            result.Libraries.Should().BeEquivalentTo(_server.Libraries);
        }
    }
}