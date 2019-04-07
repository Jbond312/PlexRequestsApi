using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Store;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;

        public RequestService(
            IRequestRepository requestRepository
            )
        {
            _requestRepository = requestRepository;
        }
        
        public async Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null)
        {
            return await _requestRepository.GetMany(filter);
        }

        public async Task<Request> GetOne(Expression<Func<Request, bool>> filter = null)
        {
            return await _requestRepository.GetOne(filter);
        }

        public async Task Create(Request request)
        {
            await _requestRepository.Create(request);
        }
    }
}