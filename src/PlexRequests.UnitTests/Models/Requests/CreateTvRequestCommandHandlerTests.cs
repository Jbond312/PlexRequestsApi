using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models.Requests;
using PlexRequests.Plex;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using Shouldly;

namespace PlexRequests.UnitTests.Models.Requests
{
    [TestFixture]
    public class CreateTvRequestCommandHandlerTests
    {
        private IRequestHandler<CreateTvRequestCommand> _underTest;

        private IRequestService _requestService;
        private ITheMovieDbApi _theMovieDbApi;
        private IPlexService _plexService;
        private IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _requestService = Substitute.For<IRequestService>();
            _theMovieDbApi = Substitute.For<ITheMovieDbApi>();
            _plexService = Substitute.For<IPlexService>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            
            _underTest = new CreateTvRequestCommandHandler(_requestService, _theMovieDbApi, _plexService, _claimsPrincipalAccessor);
            
            _fixture = new Fixture();
        }

        [Test]
        public async Task Checks_For_Existing_Request()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();

            MockExistingRequests();
            MockExternalIds();
            
            await _underTest.Handle(command, CancellationToken.None);

            await _requestService.Received().GetExistingTvRequests(Arg.Is(AgentTypes.TheMovieDb), Arg.Is(command.TheMovieDbId.ToString()));
        }

        [Test]
        public async Task Throws_Error_If_All_Episodes_Already_Requested()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            command.SeasonEpisodes = new Dictionary<int, List<int>>
            {
                [1] = new List<int> {1}
            };
            
            var existingRequests = new List<Request>
            {
                CreateExistingRequest(1, 1)
            };
            
            _requestService.GetExistingTvRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(existingRequests);

            MockExternalIds();
            
            var exception = await Should.ThrowAsync<PlexRequestException>(() => _underTest.Handle(command, CancellationToken.None));
            
            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("Request not created");
            exception.Description.ShouldBe("All TV Episodes have already been requested.");
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Gets_ExternalIds_Only_When_Not_Matched_To_TheMovieDbId(bool theMovieDbMatched)
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            
            MockExistingRequests();
            MockExternalIds();
            MockGetMediaItem(theMovieDbMatched);
            
            await _underTest.Handle(command, CancellationToken.None);

            if (theMovieDbMatched)
            {
                await _theMovieDbApi.DidNotReceive().GetTvExternalIds(Arg.Is(command.TheMovieDbId));
            }
            else
            {
                await _theMovieDbApi.Received().GetTvExternalIds(Arg.Is(command.TheMovieDbId));
            }
        }
        
        [Test]
        public async Task Lookup_Plex_MediaItem_From_TheMovieDbId()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            
            MockExistingRequests();
            MockExternalIds();
            MockGetMediaItem(true);

            await _underTest.Handle(command, CancellationToken.None);

            await _plexService.Received(1).GetExistingMediaItemByAgent(Arg.Is(PlexMediaTypes.Show), Arg.Is(AgentTypes.TheMovieDb), Arg.Is(command.TheMovieDbId.ToString()));
        }
        
        [Test]
        public async Task Lookup_From_ExternalId_TheTvDb_If_No_TheMovieDb_Match()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            
            MockExistingRequests();
            var externalIds = MockExternalIds();

            _plexService.GetExistingMediaItemByAgent(Arg.Is(PlexMediaTypes.Show), Arg.Any<AgentTypes>(), Arg.Any<string>()).ReturnsNull();
            
            await _underTest.Handle(command, CancellationToken.None);

            await _plexService.Received(1).GetExistingMediaItemByAgent(Arg.Is(PlexMediaTypes.Show), Arg.Is(AgentTypes.TheMovieDb),
                Arg.Is(command.TheMovieDbId.ToString()));
            await _plexService.Received(1).GetExistingMediaItemByAgent(Arg.Is(PlexMediaTypes.Show), Arg.Is(AgentTypes.TheTvDb),
                Arg.Is(externalIds.TvDb_Id));
        }

        [Test]
        public async Task Does_Not_Lookup_From_TheTvDb_If_No_TheMovieDb_Match_And_No_TheTvbDb_ExternalId()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            
            MockExistingRequests();
            var externalIds = MockExternalIds();
            externalIds.TvDb_Id = string.Empty;

            _plexService.GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>()).ReturnsNull();
            
            await _underTest.Handle(command, CancellationToken.None);
            
            await _plexService.ReceivedWithAnyArgs(1)
                        .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(),
                            Arg.Any<string>());
        }

        [Test]
        public async Task Throws_Error_When_Episodes_Left_In_Request_Exist_In_Plex()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            command.SeasonEpisodes = new Dictionary<int, List<int>>
            {
                [1] = new List<int> {1}
            };

            MockExistingRequests();
            MockExternalIds();
            
            var plexMediaItem = CreateExistingPlexMediaItem(1, 1);
            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(plexMediaItem);
            
            var exception = await Should.ThrowAsync<PlexRequestException>(() => _underTest.Handle(command, CancellationToken.None));
            
            exception.ShouldNotBeNull();
            exception.Message.ShouldBe("Request not created");
            exception.Description.ShouldBe("All TV Episodes hare already available in Plex.");
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Creates_New_Request()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            
            MockExistingRequests();
            MockExternalIds();

            Request actualRequest = null;
            await _requestService.Create(Arg.Do<Request>(x => actualRequest = x));
            
            await _underTest.Handle(command, CancellationToken.None);

            await _requestService.Received().Create(Arg.Any<Request>());
            actualRequest.ShouldNotBeNull();
            actualRequest.Id.ShouldBe(Guid.Empty);
            actualRequest.MediaType.ShouldBe(PlexMediaTypes.Show);
            actualRequest.IsApproved.ShouldBeFalse();
            actualRequest.AgentType.ShouldBe(AgentTypes.TheMovieDb);
            actualRequest.AgentSourceId.ShouldBe(command.TheMovieDbId.ToString());
            actualRequest.PlexRatingKey.ShouldBeNull();
            actualRequest.SeasonEpisodes.ShouldNotBeNull();
            actualRequest.SeasonEpisodes.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public async Task Creates_Request_With_Users_UserId()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            
            MockExistingRequests();
            MockExternalIds();

            var userId = _fixture.Create<Guid>();
            _claimsPrincipalAccessor.UserId.Returns(userId);
            
            await _underTest.Handle(command, CancellationToken.None);

            await _requestService.Received().Create(Arg.Is<Request>(req => req.RequestedByUserId == userId));
        }
        
        [Test]
        public async Task Creates_Request_With_Users_Username()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            
            MockExistingRequests();
            MockExternalIds();

            var username = _fixture.Create<string>();
            _claimsPrincipalAccessor.Username.Returns(username);
            
            await _underTest.Handle(command, CancellationToken.None);

            await _requestService.Received().Create(Arg.Is<Request>(req => req.RequestedByUserName == username));
        }

        [Test]
        public async Task Creates_Request_With_Episodes_Filtered_Out_By_Existing_Requests()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            command.SeasonEpisodes = new Dictionary<int, List<int>>
            {
                [1] = new List<int> {1, 2, 3}
            };
            
            var existingRequests = new List<Request>
            {
                CreateExistingRequest(1, 2)
            };
            
            _requestService.GetExistingTvRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(existingRequests);

            MockExternalIds();

            Request actualRequest = null;
            await _requestService.Create(Arg.Do<Request>(x => actualRequest = x));
            
            await _underTest.Handle(command, CancellationToken.None);
            
            actualRequest.ShouldNotBeNull();
            actualRequest.SeasonEpisodes[1].Count.ShouldBe(1);
            actualRequest.SeasonEpisodes[1].First().Episode.ShouldBe(3);
        }
        
        [Test]
        public async Task Creates_Request_With_Whole_Season_Filtered_Out_By_Existing_Requests()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            command.SeasonEpisodes = new Dictionary<int, List<int>>
            {
                [1] = new List<int> {1, 2, 3},
                [2] = new List<int> {1, 2, 3}
            };
            
            var existingRequests = new List<Request>
            {
                CreateExistingRequest(1, 3)
            };
            
            _requestService.GetExistingTvRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(existingRequests);

            MockExternalIds();

            Request actualRequest = null;
            await _requestService.Create(Arg.Do<Request>(x => actualRequest = x));
            
            await _underTest.Handle(command, CancellationToken.None);
            
            actualRequest.ShouldNotBeNull();
            actualRequest.SeasonEpisodes.ShouldNotContainKey(1);
        }
        
        [Test]
        public async Task Creates_Request_With_Episodes_Filtered_Out_By_Existing_Plex_MediaItem()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            command.SeasonEpisodes = new Dictionary<int, List<int>>
            {
                [1] = new List<int> {1, 2, 3}
            };


            MockExistingRequests();
            MockExternalIds();
            
            var plexMediaItem = CreateExistingPlexMediaItem(1, 2);
            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(plexMediaItem);
                
            Request actualRequest = null;
            await _requestService.Create(Arg.Do<Request>(x => actualRequest = x));
            
            await _underTest.Handle(command, CancellationToken.None);
            
            actualRequest.ShouldNotBeNull();
            actualRequest.SeasonEpisodes[1].Count.ShouldBe(1);
            actualRequest.SeasonEpisodes[1].First().Episode.ShouldBe(3);
        }
        
        [Test]
        public async Task Creates_Request_With_Whole_Season_Filtered_Out_By_Existing_Plex_MediaItem()
        {
            var command = _fixture.Create<CreateTvRequestCommand>();
            command.SeasonEpisodes = new Dictionary<int, List<int>>
            {
                [1] = new List<int> {1, 2, 3},
                [2] = new List<int> {1, 2, 3}
            };


            MockExistingRequests();
            MockExternalIds();

            var plexMediaItem = CreateExistingPlexMediaItem(1, 3);
            _plexService
                .GetExistingMediaItemByAgent(Arg.Any<PlexMediaTypes>(), Arg.Any<AgentTypes>(), Arg.Any<string>())
                .Returns(plexMediaItem);
                
            Request actualRequest = null;
            await _requestService.Create(Arg.Do<Request>(x => actualRequest = x));
            
            await _underTest.Handle(command, CancellationToken.None);
            
            actualRequest.ShouldNotBeNull();
            actualRequest.SeasonEpisodes.ShouldNotContainKey(1);
        }

        private void MockExistingRequests()
        {
            var existingRequests = _fixture.CreateMany<Request>().ToList();
            _requestService.GetExistingTvRequests(Arg.Any<AgentTypes>(), Arg.Any<string>()).Returns(existingRequests);
        }

        private void MockGetMediaItem(bool alreadyExists = false)
        {
            if (!alreadyExists)
            {
                _plexService.GetExistingMediaItemByAgent(Arg.Is(PlexMediaTypes.Show), Arg.Any<AgentTypes>(), Arg.Any<string>()).ReturnsNull();
            }
            else
            {
                var plexMediaItem = _fixture.Create<PlexMediaItem>();

                _plexService.GetExistingMediaItemByAgent(Arg.Is(PlexMediaTypes.Show), Arg.Any<AgentTypes>(), Arg.Any<string>())
                            .Returns(plexMediaItem);
            }
        }
        
        private ExternalIds MockExternalIds()
        {
            var externalIds = _fixture.Create<ExternalIds>();

            _theMovieDbApi.GetTvExternalIds(Arg.Any<int>()).Returns(externalIds);

            return externalIds;
        }

        private Request CreateExistingRequest(int season, int totalEpisodes)
        {
            var request = _fixture.Build<Request>()
                                  .With(x => x.SeasonEpisodes, new Dictionary<int, List<RequestEpisode>>())
                                  .Create();

            var requestEpisodes = new List<RequestEpisode>();
            for (var episode = 1; episode <= totalEpisodes; episode++)
            {
                requestEpisodes.Add(new RequestEpisode
                {
                    Episode = episode
                });
            }

            request.SeasonEpisodes = new Dictionary<int, List<RequestEpisode>>
            {
                [season] = requestEpisodes
            };

            return request;
        }
        
        private PlexMediaItem CreateExistingPlexMediaItem(int season, int totalEpisodes)
        {
            var plexMediaItem = _fixture.Build<PlexMediaItem>()
                                  .With(x => x.Seasons, new List<PlexSeason>())
                                  .Create();

            var plexEpisodes = new List<PlexEpisode>();
            for (var episode = 1; episode <= totalEpisodes; episode++)
            {
                plexEpisodes.Add(new PlexEpisode
                {
                    Episode = episode
                });
            }

            plexMediaItem.Seasons = new List<PlexSeason>
            {
                new PlexSeason
                {
                    Season = season,
                    Episodes = plexEpisodes
                }
            };

            return plexMediaItem;
        }
    }
}