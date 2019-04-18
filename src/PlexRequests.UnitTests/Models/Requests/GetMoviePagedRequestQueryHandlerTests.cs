using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PlexRequests.ApiRequests.Requests.Queries;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Mapping;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class GetMoviePagedRequestQueryHandlerTests
    {
        private readonly GetMoviePagedRequestQueryHandler _underTest;
        private readonly IRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsAccessor;

        private readonly Fixture _fixture;

        private GetMoviePagedRequestQuery _query;
        private Paged<Request> _pagedRequest;
        private Func<Task<GetMoviePagedRequestQueryResult>> _queryAction;
        private Guid _currentUserId;

        public GetMoviePagedRequestQueryHandlerTests()
        {
            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new RequestProfile()); });
            var mapper = mapperConfig.CreateMapper();
            
            _requestService = Substitute.For<IRequestService>();
            _claimsAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            
            _underTest = new GetMoviePagedRequestQueryHandler(mapper, _requestService, _claimsAccessor);

            _fixture = new Fixture();
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
                .Given(x => x.GivenAUsersClaimAccessor())
                .When(x => x.WhenQueryActionIsCreated())
                .Then(x => x.ThenQueryReturnsCorrectResponse())
                .Then(x => x.ThenCurrentUsersUserIdWasUsed())
                .BDDfy();
        }

        private void GivenAQuery()
        {
            _query = _fixture.Create<GetMoviePagedRequestQuery>();
        }

        private void GivenCurrentUsersRequestsOnly()
        {
            _query.IncludeCurrentUsersOnly = true;
        }

        private void GivenAUsersClaimAccessor()
        {
            _currentUserId = _fixture.Create<Guid>();
            _claimsAccessor.UserId.Returns(_currentUserId);
        }
        
        private void GivenManyRequests()
        {
            _pagedRequest = _fixture.Create<Paged<Request>>();
            
            _requestService.GetPaged(Arg.Any<string>(), Arg.Any<PlexMediaTypes?>(), Arg.Any<RequestStatuses?>(), Arg.Any<Guid?>(),
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

        private void ThenCurrentUsersUserIdWasUsed()
        {
            _requestService.Received().GetPaged(Arg.Any<string>(), Arg.Any<PlexMediaTypes?>(), Arg.Any<RequestStatuses?>(), Arg.Is<Guid?>(_currentUserId),
                Arg.Any<int?>(), Arg.Any<int?>());
        }
    }
}