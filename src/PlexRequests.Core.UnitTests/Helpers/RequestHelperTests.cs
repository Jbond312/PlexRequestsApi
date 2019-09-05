using System;
using AutoFixture;
using FluentAssertions;
using PlexRequests.Core.Helpers;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Helpers
{
    public class RequestHelperTests
    {
        private readonly RequestHelper _underTest;

        private readonly Fixture _fixture;

        private TvRequest _request;
        private Func<RequestStatuses> _commandAction;

        public RequestHelperTests()
        {
            _fixture = new Fixture();

            _underTest = new RequestHelper();
        }

        [Theory]
        [InlineData(RequestStatuses.Approved)]
        [InlineData(RequestStatuses.Rejected)]
        [InlineData(RequestStatuses.Completed)]
        [InlineData(RequestStatuses.PendingApproval)]
        private void AggregateStatus_Sets_Status_When_All_Of_Same_Status(RequestStatuses status)
        {
            this.Given(x => x.GivenAllRequestEpisodesOfStatus(status))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenOverallStatusIsCorrect(status))
                .BDDfy();
        }

        [Theory]
        [InlineData(RequestStatuses.Approved)]
        [InlineData(RequestStatuses.Rejected)]
        [InlineData(RequestStatuses.PendingApproval)]
        private void AggregateStatus_Sets_Status_To_PartialComplete_When_At_least_One_Episode_Complete(RequestStatuses allOtherEpisodeStatuses)
        {
            this.Given(x => x.GivenOneEpisodeOfStatus(RequestStatuses.Completed, allOtherEpisodeStatuses))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenOverallStatusIsCorrect(RequestStatuses.PartialCompletion))
                .BDDfy();
        }

        [Theory]
        [InlineData(RequestStatuses.Rejected)]
        [InlineData(RequestStatuses.PendingApproval)]
        private void AggregateStatus_Sets_Status_To_PartialApproval_When_At_least_One_Episode_Approved(RequestStatuses allOtherEpisodeStatuses)
        {
            this.Given(x => x.GivenOneEpisodeOfStatus(RequestStatuses.Approved, allOtherEpisodeStatuses))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenOverallStatusIsCorrect(RequestStatuses.PartialApproval))
                .BDDfy();
        }

        [Theory]
        [InlineData(RequestStatuses.PendingApproval)]
        [InlineData(RequestStatuses.Approved)]
        [InlineData(RequestStatuses.Completed)]
        [InlineData(RequestStatuses.PartialApproval)]
        [InlineData(RequestStatuses.PartialCompletion)]
        [InlineData(RequestStatuses.Rejected)]
        private void AggregateStatus_Sets_Status_To_Request_Status_When_Request_Is_Show_Tracking(RequestStatuses status)
        {
            this.Given(x => x.GivenTrackedRequest(status))
            .When(x => x.WhenACommandActionIsCreated())
            .Then(x => x.ThenOverallStatusIsCorrect(status))
            .BDDfy();
        }

        private void GivenAllRequestEpisodesOfStatus(RequestStatuses status)
        {
            _request = _fixture.Build<TvRequest>()
            .With(x => x.Track, false)
            .Create();

            SetEpisodeStatuses(status);
        }

        private void GivenOneEpisodeOfStatus(RequestStatuses status, RequestStatuses allOtherEpisodeStatus)
        {
            _request = _fixture.Build<TvRequest>()
            .With(x => x.Track, false)
            .Create();

            SetEpisodeStatuses(allOtherEpisodeStatus);

            _request.Seasons[0].Episodes[0].Status = status;
        }

        private void GivenTrackedRequest(RequestStatuses status)
        {
            _request = _fixture.Build<TvRequest>()
            .With(x => x.Track, true)
            .With(x => x.Status, status)
            .Create();
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = () => _underTest.CalculateAggregatedStatus(_request);
        }

        private void ThenOverallStatusIsCorrect(RequestStatuses expectedStatus)
        {
            var actualStatus = _commandAction();

            actualStatus.Should().Be(expectedStatus);
        }

        private void SetEpisodeStatuses(RequestStatuses status)
        {
            foreach (var season in _request.Seasons)
            {
                foreach (var episode in season.Episodes)
                {
                    episode.Status = status;
                }
            }
        }
    }
}