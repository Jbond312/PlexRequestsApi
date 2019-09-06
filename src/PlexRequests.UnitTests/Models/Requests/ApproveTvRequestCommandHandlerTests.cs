using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.ApiRequests.Requests.Commands;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class ApproveTvRequestCommandHandlerTests
    {
        private readonly IRequestHandler<ApproveTvRequestCommand> _underTest;
        private readonly ITvRequestService _requestService;

        private readonly Fixture _fixture;

        private ApproveTvRequestCommand _command;
        private Func<Task> _commandAction;
        private TvRequest _updatedRequest;
        private RequestStatuses _overallStatus;
        private TvRequest _request;

        public ApproveTvRequestCommandHandlerTests()
        {
            _fixture = new Fixture();

            _requestService = Substitute.For<ITvRequestService>();

            _underTest = new ApproveTvRequestCommandHandler(_requestService);
        }

        [Fact]
        private void Throws_Error_When_Invalid_Request()
        {
            const bool approveAll = false;
            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenNoRequestFound())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "No request was found with the given Id", HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        private void Throws_Error_When_Request_Already_Has_Complete_Overall_Status()
        {
            const bool approveAll = false;
            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenOverallRequestStatusIs(RequestStatuses.Completed))
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "Request has already been completed", HttpStatusCode.BadRequest))
                .BDDfy();
        }

        [Fact]
        private void When_ApproveAll_All_Season_Episodes_Are_Approved()
        {
            const bool approveAll = true;
            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenARequestIsUpdated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenAllSeasonEpisodesAreApproved())
                .BDDfy();
        }

        [Fact]
        private void When_ApproveAll_Already_Completed_Episode_Not_Altered()
        {
            const bool approveAll = true;

            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFoundWithACompletedEpisode())
                .Given(x => x.GivenARequestIsUpdated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenCompletedEpisodeNotAltered())
                .BDDfy();
        }

        [Fact]
        private void OverallStatus_Is_Calculated()
        {
            const bool approveAll = false;

            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenARequestIsUpdated())
                .Given(x => x.GivenAggregateIsCalculated())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenAggregateIsCorrect())
                .BDDfy();
        }

        [Fact]
        private void Updates_Request_Episodes_From_Command_Episodes()
        {
            const bool approveAll = false;

            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenARequestIsUpdated())
                .Given(x => x.GivenOneMatchingEpisodeInCommand())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenOnlyMatchingEpisodeApproved())
                .BDDfy();
        }

        [Fact]
        private void Approved_When_Request_Is_Tracked_Show()
        {
            const bool approveAll = false;

            this.Given(x => x.GivenACommand(approveAll))
            .Given(x => x.GivenARequestIsFound())
            .Given(x => x.GivenRequestIsTrackedShow())
            .Given(x => x.GivenARequestIsUpdated())
            .When(x => x.WhenACommandActionIsCreated())
            .Then(x => x.ThenCommandIsSuccessful())
            .Then(x => x.ThenTrackedShowIsApproved())
            .BDDfy();
        }

        private void GivenACommand(bool approveAll)
        {
            _command = _fixture.Build<ApproveTvRequestCommand>()
                               .With(x => x.ApproveAll, approveAll)
                               .Create();
        }

        private void GivenNoRequestFound()
        {
            _requestService.GetRequestById(Arg.Any<Guid>()).ReturnsNull();
        }

        private void GivenARequestIsFound()
        {
            _request = _fixture.Build<TvRequest>()
                                  .With(x => x.Status, RequestStatuses.PendingApproval)
                                  .With(x => x.Track, false)
                                  .Create();

            SetEpisodeStatus(_request, RequestStatuses.PendingApproval);

            _requestService.GetRequestById(Arg.Any<Guid>()).Returns(_request);
        }

        private void GivenRequestIsTrackedShow()
        {
            _request.Track = true;
        }

        private void GivenARequestIsFoundWithACompletedEpisode()
        {
            _request = _fixture.Build<TvRequest>()
                                  .With(x => x.Status, RequestStatuses.PendingApproval)
                                  .Create();

            SetEpisodeStatus(_request, RequestStatuses.PendingApproval);

            _request.Seasons[0].Episodes[0].Status = RequestStatuses.Completed;

            _requestService.GetRequestById(Arg.Any<Guid>()).Returns(_request);
        }

        private void GivenARequestIsUpdated()
        {
            _requestService.Update(Arg.Do<TvRequest>(x => _updatedRequest = x));
        }

        private void GivenAggregateIsCalculated()
        {
            _overallStatus = RequestStatuses.Completed;
            _requestService.CalculateAggregatedStatus(Arg.Any<TvRequest>()).Returns(_overallStatus);
        }

        private void GivenOneMatchingEpisodeInCommand()
        {
            var firstRequestSeason = _request.Seasons[0];
            var firstRequestEpisode = firstRequestSeason.Episodes[0];

            _command.EpisodesBySeason = new Dictionary<int, List<int>>
            {
                [firstRequestSeason.Index] = new List<int> { firstRequestEpisode.Index }
            };
        }

        private void GivenOverallRequestStatusIs(RequestStatuses status)
        {
            _request.Status = status;
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private void ThenAnErrorIsThrown(string expectedMessage, string expectedDescription, HttpStatusCode expectedStatusCode)
        {
            _commandAction.Should().Throw<PlexRequestException>()
                          .WithMessage(expectedMessage)
                          .Where(x => x.Description == expectedDescription)
                          .Where(x => x.StatusCode == expectedStatusCode);
        }

        private void ThenCommandIsSuccessful()
        {
            _commandAction.Should().NotThrow();
        }

        private void ThenAllSeasonEpisodesAreApproved()
        {
            _updatedRequest.Should().NotBeNull();
            _updatedRequest.Seasons.SelectMany(x => x.Episodes).All(x => x.Status == RequestStatuses.Approved).Should().BeTrue();
        }

        private void ThenCompletedEpisodeNotAltered()
        {
            _updatedRequest.Should().NotBeNull();
            var hasOneCompletedEpisode = _updatedRequest
                                         .Seasons.SelectMany(x => x.Episodes)
                                         .Count(x => x.Status == RequestStatuses.Completed);
            hasOneCompletedEpisode.Should().Be(1);
        }

        private void ThenAggregateIsCorrect()
        {
            _updatedRequest.Status.Should().Be(_overallStatus);
            _requestService.Received().CalculateAggregatedStatus(Arg.Any<TvRequest>());
        }

        private void ThenOnlyMatchingEpisodeApproved()
        {
            var updatedSeason = _command.EpisodesBySeason.First().Key;
            var updatedEpisode = _command.EpisodesBySeason.First().Value[0];

            var approvedEpisodes = _updatedRequest.Seasons.SelectMany(x => x.Episodes)
                                                  .Where(x => x.Status == RequestStatuses.Approved);

            approvedEpisodes.Count().Should().Be(1);

            var season = _updatedRequest.Seasons.First(x => x.Index == updatedSeason);

            var episode = season.Episodes.FirstOrDefault(x => x.Index == updatedEpisode);

            episode.Should().NotBeNull();
        }

        private void ThenTrackedShowIsApproved()
        {
            _updatedRequest.Should().NotBeNull();
            _updatedRequest.Status.Should().Be(RequestStatuses.Approved);
        }

        private static void SetEpisodeStatus(TvRequest request, RequestStatuses status)
        {
            foreach (var season in request.Seasons)
            {
                foreach (var episode in season.Episodes)
                {
                    episode.Status = status;
                }
            }
        }
    }
}