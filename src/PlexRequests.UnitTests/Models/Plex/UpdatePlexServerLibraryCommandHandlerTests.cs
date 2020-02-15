using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.ApiRequests.Plex.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Plex;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class UpdatePlexServerLibraryCommandHandlerTests
    {
        private readonly IRequestHandler<UpdatePlexServerLibraryCommand> _underTest;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly Fixture _fixture;
        
        private UpdatePlexServerLibraryCommand _command;
        private Func<Task> _commandAction;
        private PlexServerRow _plexServer;

        public UpdatePlexServerLibraryCommandHandlerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _plexService = Substitute.For<IPlexService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            
            _underTest = new UpdatePlexServerLibraryCommandHandler(_plexService, _unitOfWork);
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
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenLibraryIsUpdatedCorrectly())
                .Then(x => x.ThenChangesAreCommitted())
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
            _plexServer = _fixture.Create<PlexServerRow>();

            _plexServer.PlexLibraries.ElementAt(0).IsArchived = isArchived;
            
            if (isArchived)
            {
                _plexServer.PlexLibraries.ElementAt(0).LibraryKey = _command.Key;
            }
            
            _plexService.GetServer().Returns(_plexServer);
        }

        private void GivenAMatchingLibrary()
        {
            _plexServer = _fixture.Create<PlexServerRow>();
            _plexServer.PlexLibraries.ElementAt(0).LibraryKey = _command.Key;
            _plexServer.PlexLibraries.ElementAt(0).IsEnabled = false;

            _plexService.GetServer().Returns(_plexServer);
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
            var updatedLibrary = _plexServer.PlexLibraries.First(x => x.LibraryKey == _command.Key);
            updatedLibrary.IsEnabled.Should().Be(_command.IsEnabled);
        }

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }
    }
}