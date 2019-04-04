using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using NSubstitute;
using NUnit.Framework;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models.Plex;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Models;
using Shouldly;
using User = PlexRequests.Store.Models.User;

namespace PlexRequests.UnitTests.Models.Plex
{
    [TestFixture]
    public class SyncUsersCommandHandlerTests
    {
        private IRequestHandler<SyncUsersCommand> _underTest;

        private IPlexApi _plexApi;
        private IUserService _userService;
        private IPlexService _plexService;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _userService = Substitute.For<IUserService>();
            _plexService = Substitute.For<IPlexService>();

            _underTest = new SyncUsersCommandHandler(_plexApi, _userService, _plexService);

            _fixture = new Fixture();
        }

        [Test]
        public async Task Gets_Server()
        {
            var command = _fixture.Create<SyncUsersCommand>();

            MockPlexServer();
            MockPlexFriends();
            MockDbUsers();

            await _underTest.Handle(command, CancellationToken.None);

            await _plexService.Received().GetServer();
        }

        [Test]
        public async Task Gets_Plex_Friends()
        {
            var command = _fixture.Create<SyncUsersCommand>();

            var plexServer = MockPlexServer();
            MockPlexFriends();
            MockDbUsers();

            await _underTest.Handle(command, CancellationToken.None);

            await _plexApi.Received().GetFriends(Arg.Is(plexServer.AccessToken));
        }

        [Test]
        public async Task Get_Db_Users()
        {
            var command = _fixture.Create<SyncUsersCommand>();

            MockPlexServer();
            MockPlexFriends();
            MockDbUsers();

            await _underTest.Handle(command, CancellationToken.None);

            await _userService.Received().GetAllUsers();
        }

        [Test]
        public async Task Disables_Deleted_Users()
        {
            var command = _fixture.Create<SyncUsersCommand>();

            MockPlexServer();
            var remoteFriends = MockPlexFriends();
            var localUsers = MockDbUsers();
            SyncFriendsAndUsers(localUsers, remoteFriends);

            localUsers[0].Email = _fixture.Create<string>();

            await _underTest.Handle(command, CancellationToken.None);

            await _userService.Received(1).UpdateUser(localUsers[0]);
            localUsers.Count(x => x.IsDisabled).ShouldBe(1);
            localUsers[0].IsDisabled.ShouldBe(true);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public async Task Does_Not_Create_User_When_No_Email(string email)
        {
            var command = _fixture.Create<SyncUsersCommand>();

            MockPlexServer();
            var remoteFriends = MockPlexFriends();
            var localUsers = MockDbUsers();
            SyncFriendsAndUsers(localUsers, remoteFriends);

            foreach (var friend in remoteFriends)
            {
                friend.Email = email;
            }

            await _underTest.Handle(command, CancellationToken.None);

            await _userService.DidNotReceive().CreateUser(Arg.Any<User>());
        }

        [Test]
        public async Task Does_Not_Create_User_When_Already_Exists()
        {
            var command = _fixture.Create<SyncUsersCommand>();

            MockPlexServer();
            var remoteFriends = MockPlexFriends();
            var localUsers = MockDbUsers();
            SyncFriendsAndUsers(localUsers, remoteFriends);

            _userService.UserExists(Arg.Any<string>()).Returns(true);

            await _underTest.Handle(command, CancellationToken.None);

            await _userService.DidNotReceive().CreateUser(Arg.Any<User>());
        }

        [Test]
        public async Task Creates_User_Successfully()
        {
            var command = _fixture.Create<SyncUsersCommand>();

            MockPlexServer();
            var remoteFriends = MockPlexFriends();
            var localUsers = MockDbUsers();
            SyncFriendsAndUsers(localUsers, remoteFriends);

            _userService.UserExists(Arg.Any<string>()).Returns(false);

            await _underTest.Handle(command, CancellationToken.None);

            await _userService.Received(1).CreateUser(Arg.Is<User>(x => IsValidUser(x, remoteFriends[0])));
        }

        private PlexServer MockPlexServer()
        {
            var plexServer = _fixture.Create<PlexServer>();

            _plexService.GetServer().Returns(plexServer);

            return plexServer;
        }

        private List<Friend> MockPlexFriends()
        {
            var plexFriends = _fixture
                .Build<Friend>()
                .With(x => x.Id, _fixture.Create<int>().ToString)
                .CreateMany().ToList();

            _plexApi.GetFriends(Arg.Any<string>()).Returns(plexFriends);

            return plexFriends;
        }

        private List<User> MockDbUsers()
        {
            var users = _fixture.Build<User>()
                .With(x => x.IsAdmin, false)
                .With(x => x.IsDisabled, false)
                .CreateMany()
                .ToList();

            _userService.GetAllUsers().Returns(users);

            return users;
        }

        private static void SyncFriendsAndUsers(List<User> localUsers, List<Friend> remoteFriends)
        {
            for (var i = 0; i < localUsers.Count; i++)
            {
                var user = localUsers[i];
                var remoteFriend = remoteFriends[i];

                user.Email = remoteFriend.Email;
            }
        }

        private static bool IsValidUser(User user, Friend friend)
        {
            user.ShouldNotBeNull();
            user.Username.ShouldBe(friend.Username);
            user.Email.ShouldBe(friend.Email);
            user.PlexAccountId.ShouldBe(Convert.ToInt32(friend.Id));
            user.Roles.SequenceEqual(new List<string> { PlexRequestRoles.User }).ShouldBeTrue();
            return true;
        }
    }
}
