using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core;
using PlexRequests.Mapping;
using PlexRequests.Models.Requests;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class GetPagedRequestQueryHandlerTests
    {
        private readonly GetPagedRequestQueryHandler _underTest;
        private readonly IRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsAccessor;

        private readonly Fixture _fixture;

        private GetPagedRequestQuery _query;
        private Paged<Request> _pagedRequest;
        private Func<Task<GetPagedRequestQueryResult>> _queryAction;
        private Guid _currentUserId;

        public GetPagedRequestQueryHandlerTests()
        {
            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new RequestProfile()); });
            var mapper = mapperConfig.CreateMapper();
            
            _requestService = Substitute.For<IRequestService>();
            _claimsAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            
            _underTest = new GetPagedRequestQueryHandler(mapper, _requestService, _claimsAccessor);

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
            _query = _fixture.Create<GetPagedRequestQuery>();
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
            
            _requestService.GetPaged(Arg.Any<string>(), Arg.Any<PlexMediaTypes?>(), Arg.Any<bool?>(), Arg.Any<Guid?>(),
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
            _requestService.Received().GetPaged(Arg.Any<string>(), Arg.Any<PlexMediaTypes?>(), Arg.Any<bool?>(), Arg.Is<Guid?>(_currentUserId),
                Arg.Any<int?>(), Arg.Any<int?>());
        }
    }
}