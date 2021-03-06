using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Functions.Features.Requests.Queries;
using PlexRequests.Functions.Mapping;
using PlexRequests.UnitTests.Builders;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class GetMoviePagedRequestQueryHandlerTests
    {
        private readonly GetMoviePagedRequestQueryHandler _underTest;
        private readonly IMovieRequestService _requestService;

        private GetMoviePagedRequestQuery _query;
        private Paged<MovieRequestRow> _pagedRequest;
        private Func<Task<GetMoviePagedRequestQueryResult>> _queryAction;

        public GetMoviePagedRequestQueryHandlerTests()
        {
            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new RequestProfile()); });
            var mapper = mapperConfig.CreateMapper();
            
            _requestService = Substitute.For<IMovieRequestService>();
            
            _underTest = new GetMoviePagedRequestQueryHandler(mapper, _requestService);
        }

        [Fact]
        private void Returns_Requests_From_Request_Service()
        {
            this.Given(x => x.GivenAQuery())
                .Given(x => x.GivenManyRequests())
                .When(x => x.WhenQueryActionIsCreated())
                .Then(x => x.ThenQueryReturnsCorrectResponse())
                .BDDfy();
        }
        
        [Fact]
        private void Gets_Current_Users_UserId_If_Include_Users_Requests()
        {
            this.Given(x => x.GivenAQuery())
                .Given(x => x.GivenCurrentUsersRequestsOnly())
                .Given(x => x.GivenManyRequests())
                .When(x => x.WhenQueryActionIsCreated())
                .Then(x => x.ThenQueryReturnsCorrectResponse())
                .BDDfy();
        }

        private void GivenAQuery()
        {
            _query = new GetMoviePagedRequestQuery
            {
                UserInfo = new UserInfo()
            };
        }

        private void GivenCurrentUsersRequestsOnly()
        {
            _query.IncludeCurrentUsersOnly = true;
        }

        private void GivenManyRequests()
        {
            _pagedRequest = new MovieRequestRowBuilder().CreatePaged();
            
            _requestService.GetPaged(Arg.Any<string>(), Arg.Any<RequestStatuses?>(), Arg.Any<int?>(),
                Arg.Any<int?>(), Arg.Any<int?>()).Returns(_pagedRequest);
        }

        private void WhenQueryActionIsCreated()
        {
            _queryAction = async () => await _underTest.Handle(_query, CancellationToken.None);
        }

        private async Task ThenQueryReturnsCorrectResponse()
        {
            var result = await _queryAction();

            result.Should().NotBeNull();
            result.Items.Count.Should().Be(_pagedRequest.Items.Count);
            result.Should().BeEquivalentTo(_pagedRequest, options => options.ExcludingMissingMembers());
        }
    }
}