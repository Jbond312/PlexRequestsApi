using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models.Requests;
using PlexRequests.Plex;
using PlexRequests.Store.Models;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using Shouldly;

namespace PlexRequests.UnitTests.Models.Requests
{
    [TestFixture]
    public class CreateMovieRequestCommandHandlerTests
    {
        private IRequestHandler<CreateMovieRequestCommand> _underTest;

        private ITheMovieDbApi _theMovieDbApi;
        private IRequestService _requestService;
        private IPlexService _plexService;
        private IClaimsPrincipalAccessor _claimsPrincipalAccessor;
        private ILogger<CreateRequestCommandHandler> _logger;

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _theMovieDbApi = Substitute.For<ITheMovieDbApi>();
            _requestService = Substitute.For<IRequestService>();
            _plexService = Substitute.For<IPlexService>();
            _claimsPrincipalAccessor = Substitute.For<IClaimsPrincipalAccessor>();
            _logger = Substitute.For<ILogger<CreateRequestCommandHandler>>();

            _underTest = new CreateRequestCommandHandler(_theMovieDbApi, _requestService, _plexService, _claimsPrincipalAccessor, _logger);

            _fixture = new Fixture();

        }

        [Test]
        public async Task Get_ExternalIds_For_Movie()
        {
            var command = _fixture.Create<CreateMovieRequestCommand>();

            MockGetMediaItem();
            
            await _underTest.Handle(command, CancellationToken.None);

            await _theMovieDbApi.Received().GetMovieExternalIds(Arg.Is(command.TheMovieDbId));
        }

        [Test]
        public async Task Lookup_Plex_MediaItem_From_TheMovieDbId()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();

            MockGetMediaItem();
            
            await _underTest.Handle(request, CancellationToken.None);

            await _plexService.Received(1).GetOneMediaItem(Arg.Any<Expression<Func<PlexMediaItem, bool>>>());
        }

        [Test]
        public async Task Does_Not_Lookup_ImdbId_If_No_ImdbId_From_ExternalIds()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();

            _theMovieDbApi.GetMovieExternalIds(Arg.Any<int>()).ReturnsNull();
            
            MockGetMediaItem();
            await _underTest.Handle(request, CancellationToken.None);
            
            await _plexService.Received(1).GetOneMediaItem(Arg.Any<Expression<Func<PlexMediaItem, bool>>>());
        }
        
        [Test]
        public async Task Lookup_Plex_MediaItem_From_Imdb_If_Not_Found_And_Has_ImdbId()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();

            var externalIds = _fixture.Create<ExternalIds>();
            
            _theMovieDbApi.GetMovieExternalIds(Arg.Any<int>()).Returns(externalIds);

            MockGetMediaItem();

            await _underTest.Handle(request, CancellationToken.None);
            
            await _plexService.Received(2).GetOneMediaItem(Arg.Any<Expression<Func<PlexMediaItem, bool>>>());
        }

        [Test]
        public async Task Gets_Existing_Matching_Request_From_Any_User()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();
            
            MockGetMediaItem();

            await _underTest.Handle(request, CancellationToken.None);

            await _requestService.Received().GetOne(Arg.Any<Expression<Func<Request, bool>>>());
        }

        [Test]
        public async Task Throws_Error_If_Existing_Request()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();
            
            MockGetMediaItem();
            MockGetRequest();
            
            var exception = await Should.ThrowAsync<PlexRequestException>(() => _underTest.Handle(request, CancellationToken.None));
            
            exception.Message.ShouldBe("Request not created");
            exception.Description.ShouldBe("The Movie has already been requested.");
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
        
        [Test]
        public async Task Throws_Error_New_Request_But_MediaItem_Already_In_Plex()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();
            
            MockGetMediaItem(alreadyExists: true);
            
            var exception = await Should.ThrowAsync<PlexRequestException>(() => _underTest.Handle(request, CancellationToken.None));
            
            exception.Message.ShouldBe("Request not created");
            exception.Description.ShouldBe("The Movie is already available in Plex.");
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Creates_New_Request()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();
            
            MockGetMediaItem();

            await _underTest.Handle(request, CancellationToken.None);

            await _requestService.Received().Create(Arg.Any<Request>());
        }

        [Test]
        public async Task Gets_Username_From_ClaimsPrincipal()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();
            
            MockGetMediaItem();

            var username = _fixture.Create<string>();
            _claimsPrincipalAccessor.Username.Returns(username);

            await _underTest.Handle(request, CancellationToken.None);

            await _requestService.Received().Create(Arg.Is<Request>(req => req.RequestedByUserName == username));
        }
        
        [Test]
        public async Task Gets_UserId_From_ClaimsPrincipal()
        {
            var request = _fixture.Create<CreateMovieRequestCommand>();
            
            MockGetMediaItem();

            var userId = _fixture.Create<Guid>();
            _claimsPrincipalAccessor.UserId.Returns(userId);

            await _underTest.Handle(request, CancellationToken.None);

            await _requestService.Received().Create(Arg.Is<Request>(req => req.RequestedByUserId == userId));
        }

        private void MockGetMediaItem(bool alreadyExists = false)
        {
            if (!alreadyExists)
            {
                _plexService.GetOneMediaItem(Arg.Any<Expression<Func<PlexMediaItem, bool>>>()).ReturnsNull();
            }
            else
            {
                var plexMediaItem = _fixture.Create<PlexMediaItem>();

                _plexService.GetOneMediaItem(Arg.Any<Expression<Func<PlexMediaItem, bool>>>())
                            .Returns(plexMediaItem);
            }
        }

        private void MockGetRequest()
        {
            var request = _fixture.Create<Request>();

            _requestService.GetOne(Arg.Any<Expression<Func<Request, bool>>>()).Returns(request);
        }
    }
}