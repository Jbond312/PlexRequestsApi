using System;
using System.Linq;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Helpers
{
    public class RequestHelper : IRequestHelper
    {
        public RequestStatuses CalculateAggregatedStatus(Request request)
        {
            var possibleStatus = Enum.GetValues(typeof(RequestStatuses)).Cast<RequestStatuses>();
            var statusCounts = possibleStatus.ToDictionary(status => status, count => 0);

            foreach (var season in request.Seasons)
            {
                foreach (var episode in season.Episodes)
                {
                    statusCounts[episode.Status]++;
                }
            }

            var totalStatusCounts = statusCounts.Sum(x => x.Value);
            var overallStatus = RequestStatuses.PendingApproval;

            foreach (var (status, count) in statusCounts.OrderBy(x => x.Key))
            {
                if (count == totalStatusCounts)
                {
                    overallStatus = status;
                    break;
                }
                
                if (count == 0 || status == RequestStatuses.Rejected)
                {
                    continue;
                }

                overallStatus = status == RequestStatuses.Approved ? RequestStatuses.PartialApproval : RequestStatuses.PartialCompletion;
            }

            return overallStatus;
        }
    }
}