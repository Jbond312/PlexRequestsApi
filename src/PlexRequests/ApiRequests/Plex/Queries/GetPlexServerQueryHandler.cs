using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.ApiRequests.Plex.DTOs.Detail;
using PlexRequests.Plex;

namespace PlexRequests.ApiRequests.Plex.Queries
{
    public class GetPlexServerQueryHandler : IRequestHandler<GetServerQuery, GetPlexServerQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IPlexService _plexService;

        public GetPlexServerQueryHandler(
            IMapper mapper,
            IPlexService plexService
        )
        {
            _mapper = mapper;
            _plexService = plexService;
        }

        public async Task<GetPlexServerQueryResult> Handle(GetServerQuery request, CancellationToken cancellationToken)
        {
            var server = await _plexService.GetServer();
            var serverModel = _mapper.Map<PlexServerDetailModel>(server);

            return new GetPlexServerQueryResult
            {
                Server = serverModel
            };
        }
    }
}
