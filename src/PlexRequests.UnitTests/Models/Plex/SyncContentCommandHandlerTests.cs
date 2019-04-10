using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.Models.Plex;
using PlexRequests.Sync;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class SyncContentCommandHandlerTests
    {
        private readonly IRequestHandler<SyncContentCommand> _underTest;

        private readonly IPlexSync _plexSync;

        private readonly Fixture _fixture;
        
        private SyncContentCommand _command;
        private Func<Task> _commandAction;

        public SyncContentCommandHandlerTests()
        {
            _plexSync = Substitute.For<IPlexSync>();

            _underTest = new SyncContentCommandHandler(_plexSync);

            _fixture = new Fixture();
        }

        [Fact]
        private void Invokes_Plex_Sync()
        {
            this.Given(x => x.GivenACommand())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenPlexSyncShouldBeInvoked())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<SyncContentCommand>();
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenPlexSyncShouldBeInvoked()
        {
            _commandAction.Should().NotThrow();

            _plexSync.Received().Synchronise(Arg.Is(_command.FullRefresh));
        }
    }
}
