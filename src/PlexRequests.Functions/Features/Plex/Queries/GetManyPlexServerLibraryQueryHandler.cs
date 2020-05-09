using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Functions.Features.Plex.Models.Detail;
using PlexRequests.Plex;

namespace PlexRequests.Functions.Features.Plex.Queries
{
    public class GetManyPlexServerLibraryQueryHandler : IRequestHandler<GetManyPlexServerLibraryQuery, ValidationContext<GetManyPlexServerLibraryQueryResult>>
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

        public async Task<ValidationContext<GetManyPlexServerLibraryQueryResult>> Handle(GetManyPlexServerLibraryQuery request, CancellationToken cancellationToken)
        {
            var result = new ValidationContext<GetManyPlexServerLibraryQueryResult>();

            var server = await _plexService.GetServer();

            if (server == null)
            {
                result.AddError("No admin server found", "Cannot sync libraries as no admin server has been found");
                return result;
            }

            result.Data = new GetManyPlexServerLibraryQueryResult
            {
                Libraries = _mapper.Map<List<PlexServerLibraryDetailModel>>(server.PlexLibraries)
            };

            return result;
        }
    }
}