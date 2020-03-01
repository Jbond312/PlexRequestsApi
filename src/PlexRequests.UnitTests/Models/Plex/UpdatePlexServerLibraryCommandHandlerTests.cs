using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Plex.Commands;
using PlexRequests.Plex;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class UpdatePlexServerLibraryCommandHandlerTests
    {
        private readonly IRequestHandler<UpdatePlexServerLibraryCommand, ValidationContext> _underTest;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;

        private UpdatePlexServerLibraryCommand _command;
        private Func<Task<ValidationContext>> _commandAction;
        private PlexServerRow _plexServer;

        public UpdatePlexServerLibraryCommandHandlerTests()
        {
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
                .Then(x => x.ThenAnErrorIsThrown("Invalid library key", "No library was found for the given key"))
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
            _command = new UpdatePlexServerLibraryCommand
            {
                IsEnabled = true
            };
        }

        private void GivenNoMatchingLibrary(bool isArchived)
        {
            _plexServer = new PlexServerRowBuilder().WithLibraries().Build();

            _plexServer.PlexLibraries.ElementAt(0).IsArchived = isArchived;
            
            if (isArchived)
            {
                _plexServer.PlexLibraries.ElementAt(0).LibraryKey = _command.Key;
            }
            
            _plexService.GetServer().Returns(_plexServer);
        }

        private void GivenAMatchingLibrary()
        {
            _plexServer = new PlexServerRowBuilder().WithLibraries().Build();
            _plexServer.PlexLibraries.ElementAt(0).LibraryKey = _command.Key;
            _plexServer.PlexLibraries.ElementAt(0).IsEnabled = false;

            _plexService.GetServer().Returns(_plexServer);
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private async Task ThenAnErrorIsThrown(string message, string description)
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeFalse();
            var firstError = result.ValidationErrors.First();
            firstError.Message.Should().Be(message);
            firstError.Description.Should().Be(description);
        }

        private async Task ThenCommandIsSuccessful()
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeTrue();
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