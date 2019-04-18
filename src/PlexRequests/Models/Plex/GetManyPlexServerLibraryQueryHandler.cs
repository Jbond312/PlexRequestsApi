using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Models.SubModels.Detail;
using PlexRequests.Plex;

namespace PlexRequests.Models.Plex
{
    public class GetManyPlexServerLibraryQueryHandler : IRequestHandler<GetManyPlexServerLibraryQuery, GetManyPlexServerLibraryQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IPlexService _plexService;

        public GetManyPlexServerLibraryQueryHandler(
            IMapper mapper, 
            IPlexService plexService)
        {
            _mapper = mapper;
            _plexService = plexService;
        }
        
        public async Task<GetManyPlexServerLibraryQueryResult> Handle(GetManyPlexServerLibraryQuery request, CancellationToken cancellationToken)
        {
           var server = await _plexService.GetServer();

           return new GetManyPlexServerLibraryQueryResult
           {
               Libraries = _mapper.Map<List<PlexServerLibraryDetailModel>>(server.Libraries)
           };
        }
    }
}