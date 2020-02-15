using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using PlexRequests.Core.Helpers;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.Core.UnitTests.Helpers
{
    public class RequestHelperTests
    {
        private readonly RequestHelper _underTest;

        private readonly Fixture _fixture;

        private TvRequestRow _request;
        private Action _commandAction;

        public RequestHelperTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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
                .Then(x => x.ThenTheCommandSucceeds())
                .Then(x => x.ThenSeasonStatusIsCorrect(1, status))
                .Then(x => x.ThenSeasonStatusIsCorrect(2, status))
                .Then(x => x.ThenSeasonStatusIsCorrect(3, status))
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
                .Then(x => x.ThenTheCommandSucceeds())
                .Then(x => x.ThenSeasonStatusIsCorrect(1, RequestStatuses.PartialCompletion))
                .Then(x => x.ThenSeasonStatusIsCorrect(2, allOtherEpisodeStatuses))
                .Then(x => x.ThenSeasonStatusIsCorrect(3, allOtherEpisodeStatuses))
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
                .Then(x => x.ThenTheCommandSucceeds())
                .Then(x => x.ThenSeasonStatusIsCorrect(1, RequestStatuses.PartialApproval))
                .Then(x => x.ThenSeasonStatusIsCorrect(2, allOtherEpisodeStatuses))
                .Then(x => x.ThenSeasonStatusIsCorrect(3, allOtherEpisodeStatuses))
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
            .Given(x => x.GivenAllRequestEpisodesOfStatus(status))
            .When(x => x.WhenACommandActionIsCreated())
            .Then(x => x.ThenTheCommandSucceeds())
            .Then(x => x.ThenSeasonStatusIsCorrect(1, status))
            .Then(x => x.ThenSeasonStatusIsCorrect(2, status))
            .Then(x => x.ThenSeasonStatusIsCorrect(3, status))
            .Then(x => x.ThenOverallStatusIsCorrect(status))
            .BDDfy();
        }

        private void GivenAllRequestEpisodesOfStatus(RequestStatuses status)
        {
            _request = _fixture.Build<TvRequestRow>()
            .With(x => x.Track, false)
            .Create();

            SetEpisodeStatuses(status);
        }

        private void GivenOneEpisodeOfStatus(RequestStatuses status, RequestStatuses allOtherEpisodeStatus)
        {
            _request = _fixture.Build<TvRequestRow>()
            .With(x => x.Track, false)
            .Create();

            SetEpisodeStatuses(allOtherEpisodeStatus);

            _request.TvRequestSeasons.ElementAt(0).TvRequestEpisodes.ElementAt(0).RequestStatus = status;
        }

        private void GivenTrackedRequest(RequestStatuses status)
        {
            _request = _fixture.Build<TvRequestRow>()
            .With(x => x.Track, true)
            .With(x => x.RequestStatus, status)
            .Create();
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = () => _underTest.SetAggregatedStatus(_request);
        }

        private void ThenOverallStatusIsCorrect(RequestStatuses expectedStatus)
        {
            _request.RequestStatus.Should().Be(expectedStatus);
        }

        private void ThenSeasonStatusIsCorrect(int season, RequestStatuses expectedStatus)
        {
            _request.TvRequestSeasons.ElementAt(season - 1).RequestStatus.Should().Be(expectedStatus);
        }

        private void ThenTheCommandSucceeds()
        {
            _commandAction();
        }

        private void SetEpisodeStatuses(RequestStatuses status)
        {
            foreach (var season in _request.TvRequestSeasons)
            {
                foreach (var episode in season.TvRequestEpisodes)
                {
                    episode.RequestStatus = status;
                }
            }
        }
    }
}