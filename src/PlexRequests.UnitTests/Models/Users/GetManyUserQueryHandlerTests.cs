using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Users.Queries;
using PlexRequests.Functions.Mapping;
using PlexRequests.UnitTests.Builders;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Users
{
    public class GetManyUserQueryHandlerTests
    {
        private GetManyUserQueryHandler _underTest;
        private IUserService _userService;
        private IClaimsPrincipalAccessor _claimsAccessor;

        private List<UserRow> _users = new List<UserRow>();
        private GetManyUserQuery _query = new GetManyUserQuery();
        private Func<Task<ValidationContext<GetManyUserQueryResult>>> _queryAction;

        public GetManyUserQueryHandlerTests()
        {
            _userService = Substitute.For<IUserService>();
            _claimsAccessor = Substitute.For<IClaimsPrincipalAccessor>();

            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new UserProfile()); });
            var mapper = mapperConfig.CreateMapper();
            _underTest = new GetManyUserQueryHandler(mapper, _userService, _claimsAccessor);
        }

        [Fact]
        public void Returns_Empty_Collection_When_No_Users_Returned()
        {
            this.Given(x => x.Given_No_Users_Returned())
            .When(x => x.When_Query_Is_Created())
            .Then(x => x.Then_Response_Is_Correct())
            .BDDfy();
        }

        [Fact]
        public void Returns_Users_Correctly()
        {
            this.Given(x => x.Given_Users_Returned())
            .Given(x => x.Given_ClaimsProcessor_Returns_UserId_Not_In_List())
            .When(x => x.When_Query_Is_Created())
            .Then(x => x.Then_Response_Is_Correct())
            .BDDfy();
        }

        [Fact]
        public void Excludes_User_That_Matches_Current_UserId()
        {
            this.Given(x => x.Given_Users_Returned())
            .Given(x => x.Given_ClaimsProcessor_Matches_A_UserId())
            .When(x => x.When_Query_Is_Created())
            .Then(x => x.Then_Matching_User_Is_Skipped())
            .BDDfy();
        }

        private void Given_ClaimsProcessor_Returns_UserId_Not_In_List()
        {
            var maxUserId = !_users.Any() ? 0 : _users.Max(x => x.UserId) + 1;
            _claimsAccessor.UserId.Returns(maxUserId);
        }

        private void Given_ClaimsProcessor_Matches_A_UserId()
        {
            _claimsAccessor.UserId.Returns(_users.First().UserId);
        }

        private void Given_No_Users_Returned()
        {
            _userService.GetAllUsers(Arg.Any<bool>()).Returns(new List<UserRow>());
        }

        private void Given_Users_Returned()
        {
            _users = new UserRowBuilder().CreateMany();
            _userService.GetAllUsers(Arg.Any<bool>()).Returns(_users);
        }

        private void When_Query_Is_Created()
        {
            _queryAction = async () => await _underTest.Handle(_query, CancellationToken.None);
        }

        private async Task Then_Response_Is_Correct()
        {
            var result = await _queryAction();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Users.Count.Should().Equals(_users.Count);
        }

        private async Task Then_Matching_User_Is_Skipped()
        {
            var result = await _queryAction();
            result.IsSuccessful.Should().BeTrue();
            result.Data.Users.Count.Should().Equals(_users.Count - 1);
        }

    }
}