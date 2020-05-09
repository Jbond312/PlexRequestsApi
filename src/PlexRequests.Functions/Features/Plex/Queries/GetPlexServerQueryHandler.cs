using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Functions.Features.Plex.Models.Detail;
using PlexRequests.Plex;

namespace PlexRequests.Functions.Features.Plex.Queries
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

            if (server == null)
            {
                return null;
            }

            var serverModel = _mapper.Map<PlexServerDetailModel>(server);

            return new GetPlexServerQueryResult
            {
                Server = serverModel
            };
        }
    }
}
