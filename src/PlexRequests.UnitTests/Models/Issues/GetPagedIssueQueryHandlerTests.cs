using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Issues.Queries;
using PlexRequests.Functions.Mapping;
using PlexRequests.UnitTests.Builders;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Issues
{
    public class GetPagedIssueQueryHandlerTests
    {
        private readonly GetPagedIssueQueryHandler _underTest;
        private readonly IIssueService _issueService;

        private GetPagedIssueQuery _query;
        private Paged<IssueRow> _pagedIssue;
        private List<IssueStatuses> _includedStatuses;
        private Func<Task<ValidationContext<GetPagedIssueQueryResult>>> _queryAction;

        public GetPagedIssueQueryHandlerTests()
        {
            var mapperConfig = new MapperConfiguration(opts => { opts.AddProfile(new IssueProfile()); });
            var mapper = mapperConfig.CreateMapper();

            _issueService = Substitute.For<IIssueService>();

            _underTest = new GetPagedIssueQueryHandler(mapper, _issueService);
        }

        [Fact]
        private void Returns_Issues_From_Issue_Service()
        {
            bool includeResolved = false;
            this.Given(x => x.GivenAQuery(includeResolved))
                .Given(x => x.GivenManyIssues())
                .When(x => x.WhenQueryActionIsCreated())
                .Then(x => x.ThenQueryReturnsCorrectResponse())
                .Then(x => x.ThenResolvedStatusWasNotIncluded())
                .BDDfy();
        }

        [Fact]
        private void Includes_Resolved_When_Requested()
        {
            bool includeResolved = true;
            this.Given(x => x.GivenAQuery(includeResolved))
                .Given(x => x.GivenManyIssues())
                .When(x => x.WhenQueryActionIsCreated())
                .Then(x => x.ThenQueryReturnsCorrectResponse())
                .Then(x => x.ThenResolvedStatusWasIncluded())
                .BDDfy();
        }


        private void GivenAQuery(bool includeResolved)
        {
            _query = new GetPagedIssueQuery
            {
                IncludeResolved = includeResolved
            };
        }

        private void GivenManyIssues()
        {
            _pagedIssue = new IssueRowBuilder().CreatePaged();
            _issueService.GetPaged(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Do<List<IssueStatuses>>(x => _includedStatuses = x)).Returns(_pagedIssue);
        }

        private void WhenQueryActionIsCreated()
        {
            _queryAction = async () => await _underTest.Handle(_query, CancellationToken.None);
        }

        private void ThenResolvedStatusWasIncluded()
        {
            _includedStatuses.Should().Contain(IssueStatuses.Resolved);
        }

        private void ThenResolvedStatusWasNotIncluded()
        {
            _includedStatuses.Should().NotContain(IssueStatuses.Resolved);
        }

        private async Task ThenQueryReturnsCorrectResponse()
        {
            var result = await _queryAction();

            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
            result.Data.Items.Count.Should().Be(_pagedIssue.Items.Count);
            result.Should().BeEquivalentTo(_pagedIssue, options => options.ExcludingMissingMembers());
        }
    }
}