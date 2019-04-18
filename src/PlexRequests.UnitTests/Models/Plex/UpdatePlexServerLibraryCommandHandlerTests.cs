using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.Core.Exceptions;
using PlexRequests.Models.Plex;
using PlexRequests.Plex;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class UpdatePlexServerLibraryCommandHandlerTests
    {
        private readonly IRequestHandler<UpdatePlexServerLibraryCommand> _underTest;
        private readonly IPlexService _plexService;

        private readonly Fixture _fixture;
        
        private UpdatePlexServerLibraryCommand _command;
        private Func<Task> _commandAction;
        private PlexServer _updatedServer;

        public UpdatePlexServerLibraryCommandHandlerTests()
        {
            _fixture = new Fixture();

            _plexService = Substitute.For<IPlexService>();
            
            _underTest = new UpdatePlexServerLibraryCommandHandler(_plexService);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]

        private void Throws_Error_If_Invalid_Plex_Server_Library(bool isArchived)
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenNoMatchingLibrary(isArchived))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid library key", "No library was found for the given key", HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        private void Updates_Library_Correctly()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAMatchingLibrary())
                .Given(x => x.GivenALibraryIsUpdated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenLibraryIsUpdatedCorrectly())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Build<UpdatePlexServerLibraryCommand>()
                               .With(x => x.IsEnabled, true)
                               .Create();
        }

        private void GivenNoMatchingLibrary(bool isArchived)
        {
            var plexServer = _fixture.Create<PlexServer>();

            plexServer.Libraries[0].IsArchived = isArchived;
            
            if (isArchived)
            {
                plexServer.Libraries[0].Key = _command.Key;
            }
            
            _plexService.GetServer().Returns(plexServer);
        }

        private void GivenAMatchingLibrary()
        {
            var plexServer = _fixture.Create<PlexServer>();
            plexServer.Libraries[0].Key = _command.Key;
            plexServer.Libraries[0].IsEnabled = false;

            _plexService.GetServer().Returns(plexServer);
        }

        private void GivenALibraryIsUpdated()
        {
            _plexService.Update(Arg.Do<PlexServer>(x => _updatedServer = x));
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenAnErrorIsThrown(string message, string description, HttpStatusCode statusCode)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(message)
                          .Where(x => x.Description == description)
                          .Where(x => x.StatusCode == statusCode);
        }

        private void ThenCommandIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenLibraryIsUpdatedCorrectly()
        {
            _updatedServer.Should().NotBeNull();
            var updatedLibrary = _updatedServer.Libraries.First(x => x.Key == _command.Key);
            updatedLibrary.IsEnabled.Should().Be(_command.IsEnabled);
        }
    }
}