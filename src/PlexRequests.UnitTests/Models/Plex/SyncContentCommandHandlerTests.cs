using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Models.Plex;
using PlexRequests.Sync;

namespace PlexRequests.UnitTests.Models.Plex
{
    [TestFixture]
    public class SyncContentCommandHandlerTests
    {
        private IRequestHandler<SyncContentCommand> _underTest;

        private IPlexSync _plexSync;

        private Fixture _fixture;
        

        [SetUp]
        public void Setup()
        {
            _plexSync = Substitute.For<IPlexSync>();

            _underTest = new SyncContentCommandHandler(_plexSync);

            _fixture = new Fixture();
        }

        [Test]
        public async Task Calls_Plex_Synchronisation()
        {
            var command = _fixture.Create<SyncContentCommand>();

            await _underTest.Handle(command, CancellationToken.None);

            await _plexSync.Received(1).Synchronise(Arg.Is(command.FullRefresh));
        }
    }
}
