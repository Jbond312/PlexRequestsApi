using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRequests.Functions.Features;
using PlexRequests.Functions.Features.Requests.Commands;
using PlexRequests.UnitTests.Builders.DataAccess;
using TestStack.BDDfy;
using Xunit;

namespace PlexRequests.UnitTests.Models.Requests
{
    public class ApproveTvRequestCommandHandlerTests
    {
        private readonly IRequestHandler<ApproveTvRequestCommand, ValidationContext> _underTest;
        private readonly ITvRequestService _requestService;
        private readonly IUnitOfWork _unitOfWork;

        private ApproveTvRequestCommand _command;
        private Func<Task<ValidationContext>> _commandAction;
        private TvRequestRow _request;

        public ApproveTvRequestCommandHandlerTests()
        {
            _requestService = Substitute.For<ITvRequestService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _underTest = new ApproveTvRequestCommandHandler(_requestService, _unitOfWork);
        }

        [Fact]
        private void Throws_Error_When_Invalid_Request()
        {
            const bool approveAll = false;
            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenNoRequestFound())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "No request was found with the given Id"))
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
                .Then(x => x.ThenAnErrorIsThrown("Invalid request", "Request has already been completed"))
                .BDDfy();
        }

        [Fact]
        private void When_ApproveAll_All_Season_Episodes_Are_Approved()
        {
            const bool approveAll = true;
            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFound())
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
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenCompletedEpisodeNotAltered())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void OverallStatus_Is_Calculated()
        {
            const bool approveAll = false;

            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFound())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenAggregateIsCorrect())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void Updates_Request_Episodes_From_Command_Episodes()
        {
            const bool approveAll = false;

            this.Given(x => x.GivenACommand(approveAll))
                .Given(x => x.GivenARequestIsFound())
                .Given(x => x.GivenOneMatchingEpisodeInCommand())
                .When(x => x.WhenACommandActionIsCreated())
                .Then(x => x.ThenCommandIsSuccessful())
                .Then(x => x.ThenOnlyMatchingEpisodeApproved())
                .Then(x => x.ThenChangesAreCommitted())
                .BDDfy();
        }

        [Fact]
        private void Approved_When_Request_Is_Tracked_Show()
        {
            const bool approveAll = false;

            this.Given(x => x.GivenACommand(approveAll))
            .Given(x => x.GivenARequestIsFound())
            .Given(x => x.GivenRequestIsTrackedShow())
            .When(x => x.WhenACommandActionIsCreated())
            .Then(x => x.ThenCommandIsSuccessful())
            .Then(x => x.ThenTrackedShowIsApproved())
            .Then(x => x.ThenChangesAreCommitted())
            .BDDfy();
        }

        private void GivenACommand(bool approveAll)
        {
            _command = new ApproveTvRequestCommand
            {
                ApproveAll = approveAll,
                RequestId = 1,
                EpisodesBySeason = new Dictionary<int, List<int>>
                {
                    [1] = new List<int> {1, 2, 3},
                    [2] = new List<int> {1, 2, 3, 4, 56}
                }
            };
        }

        private void GivenNoRequestFound()
        {
            _requestService.GetRequestById(Arg.Any<int>()).ReturnsNull();
        }

        private void GivenARequestIsFound()
        {
            _request = new TvRequestRowBuilder().WithRequestStatus(RequestStatuses.PendingApproval).WithTrack(false).Build();
            
            SetEpisodeStatus(_request, RequestStatuses.PendingApproval);

            _requestService.GetRequestById(Arg.Any<int>()).Returns(_request);
        }

        private void GivenRequestIsTrackedShow()
        {
            _request.Track = true;
        }

        private void GivenARequestIsFoundWithACompletedEpisode()
        {
            _request = new TvRequestRowBuilder().WithRequestStatus(RequestStatuses.PendingApproval).WithTrack(false).Build();

            SetEpisodeStatus(_request, RequestStatuses.PendingApproval);

            _request.TvRequestSeasons.ElementAt(0).TvRequestEpisodes.ElementAt(0).RequestStatus = RequestStatuses.Completed;

            _requestService.GetRequestById(Arg.Any<int>()).Returns(_request);
        }
        private void GivenOneMatchingEpisodeInCommand()
        {
            var firstRequestSeason = _request.TvRequestSeasons.ElementAt(0);
            var firstRequestEpisode = firstRequestSeason.TvRequestEpisodes.ElementAt(0);

            _command.EpisodesBySeason = new Dictionary<int, List<int>>
            {
                [firstRequestSeason.SeasonIndex] = new List<int> { firstRequestEpisode.EpisodeIndex }
            };
        }

        private void GivenOverallRequestStatusIs(RequestStatuses status)
        {
            _request.RequestStatus = status;
        }

        private void WhenACommandActionIsCreated()
        {
            _commandAction = async () => await _underTest.Handle(_command, CancellationToken.None);
        }

        private async Task ThenAnErrorIsThrown(string expectedMessage, string expectedDescription)
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeFalse();
            var firstError = result.ValidationErrors[0];
            firstError.Message.Should().Be(expectedMessage);
            firstError.Description.Should().Be(expectedDescription);
        }

        private async Task ThenCommandIsSuccessful()
        {
            var result = await _commandAction();
            result.IsSuccessful.Should().BeTrue();
        }

        private void ThenAllSeasonEpisodesAreApproved()
        {
            _request.Should().NotBeNull();
            _request.TvRequestSeasons.SelectMany(x => x.TvRequestEpisodes).All(x => x.RequestStatus == RequestStatuses.Approved).Should().BeTrue();
        }

        private void ThenCompletedEpisodeNotAltered()
        {
            _request.Should().NotBeNull();
            var hasOneCompletedEpisode = _request
                                         .TvRequestSeasons.SelectMany(x => x.TvRequestEpisodes)
                                         .Count(x => x.RequestStatus == RequestStatuses.Completed);
            hasOneCompletedEpisode.Should().Be(1);
        }

        private void ThenAggregateIsCorrect()
        {
            _requestService.Received().SetAggregatedStatus(Arg.Any<TvRequestRow>());
        }

        private void ThenOnlyMatchingEpisodeApproved()
        {
            var updatedSeason = _command.EpisodesBySeason.First().Key;
            var updatedEpisode = _command.EpisodesBySeason.First().Value[0];

            var approvedEpisodes = _request.TvRequestSeasons.SelectMany(x => x.TvRequestEpisodes)
                                                  .Where(x => x.RequestStatus == RequestStatuses.Approved);

            approvedEpisodes.Count().Should().Be(1);

            var season = _request.TvRequestSeasons.First(x => x.SeasonIndex == updatedSeason);

            var episode = season.TvRequestEpisodes.FirstOrDefault(x => x.EpisodeIndex == updatedEpisode);

            episode.Should().NotBeNull();
        }

        private void ThenTrackedShowIsApproved()
        {
            _request.Should().NotBeNull();
            _request.RequestStatus.Should().Be(RequestStatuses.Approved);
        }

        private void ThenChangesAreCommitted()
        {
            _unitOfWork.Received(1).CommitAsync();
        }

        private static void SetEpisodeStatus(TvRequestRow request, RequestStatuses status)
        {
            foreach (var season in request.TvRequestSeasons)
            {
                foreach (var episode in season.TvRequestEpisodes)
                {
                    episode.RequestStatus = status;
                }
            }
        }
    }
}