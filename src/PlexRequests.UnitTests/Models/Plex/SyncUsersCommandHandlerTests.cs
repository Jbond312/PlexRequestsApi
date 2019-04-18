using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using PlexRequests.ApiRequests.Plex.Commands;
using PlexRequests.Core.Services;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;
using User = PlexRequests.Repository.Models.User;

namespace PlexRequests.UnitTests.Models.Plex
{
    public class SyncUsersCommandHandlerTests
    {
        private readonly IRequestHandler<SyncUsersCommand> _underTest;

        private readonly IPlexApi _plexApi;
        private readonly IUserService _userService;
        private readonly IPlexService _plexService;

        private readonly Fixture _fixture;
        
        private SyncUsersCommand _command;
        private List<Friend> _remoteFriends;
        private List<User> _localUsers;
        private Func<Task> _commandAction;

        public SyncUsersCommandHandlerTests()
        {
            _plexApi = Substitute.For<IPlexApi>();
            _userService = Substitute.For<IUserService>();
            _plexService = Substitute.For<IPlexService>();

            _underTest = new SyncUsersCommandHandler(_plexApi, _userService, _plexService);

            _fixture = new Fixture();
        }

        [Fact]
        public void Disables_Local_Users_When_In_Local_But_Not_Remote()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAServer())
                .Given(x => x.GivenRemoteFriends(false))
                .Given(x => x.GivenLocalUsers(true))
                .When(x => x.WhenACommandIsCreated())
                .Then(x => x.ThenLocalUsersAreDisabled())
                .BDDfy();
        }

        [Fact]
        public void Creates_Local_Users_When_In_Remote_But_Not_Local()
        {
            this.Given(x => x.GivenACommand())
                .Given(x => x.GivenAServer())
                .Given(x => x.GivenRemoteFriends(true))
                .Given(x => x.GivenLocalUsers(false))
                .When(x => x.WhenACommandIsCreated())
                .Then(x => x.ThenLocalUsersAreCreated())
                .BDDfy();
        }

        private void GivenACommand()
        {
            _command = _fixture.Create<SyncUsersCommand>();
        }

        private void GivenAServer()
        {
            _plexService.GetServer().Returns(_fixture.Create<PlexServer>());
        }

        private void GivenRemoteFriends(bool hasRemoteFriends)
        {
            _remoteFriends = new List<Friend>();

            if (hasRemoteFriends)
            {
                _remoteFriends = _fixture.Build<Friend>()
                                         .With(x => x.Id, _fixture.Create<int>().ToString)
                                         .CreateMany()
                                         .ToList();
            }
            
            _plexApi.GetFriends(Arg.Any<string>()).Returns(_remoteFriends);
        }

        private void GivenLocalUsers(bool hasLocalUsers)
        {
            _localUsers = new List<User>();

            if (hasLocalUsers)
            {
                _localUsers = _fixture.Build<User>()
                                      .With(x => x.IsAdmin, false)
                                      .CreateMany()
                                      .ToList();
            }

            _userService.GetAllUsers().Returns(_localUsers);
        }

        private void WhenACommandIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenLocalUsersAreCreated()
        {
            _commandAction.Should().NotThrow();

            _userService.Received(_remoteFriends.Count).CreateUser(Arg.Any<User>());
        }

        private void ThenLocalUsersAreDisabled()
        {
            _commandAction.Should().NotThrow();

            _userService.Received(_localUsers.Count).UpdateUser(Arg.Any<User>());
        }
    }
}
