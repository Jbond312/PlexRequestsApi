using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Helpers
{
    public class RequestHelper : IRequestHelper
    {
        private IEnumerable<RequestStatuses> _allStatuses = Enum.GetValues(typeof(RequestStatuses)).Cast<RequestStatuses>();

        public void SetAggregatedStatus(TvRequest request)
        {
            if (request.Track)
            {
                return;
            }

            var allStatusCounts = _allStatuses.ToDictionary(status => status, count => 0);

            foreach (var season in request.Seasons)
            {
                var seasonStatusCounts = _allStatuses.ToDictionary(status => status, count => 0);

                foreach (var episode in season.Episodes)
                {
                    allStatusCounts[episode.Status]++;
                    seasonStatusCounts[episode.Status]++;
                }

                season.Status = CalculateStatus(seasonStatusCounts);
            }

            request.Status = CalculateStatus(allStatusCounts);
        }

        private RequestStatuses CalculateStatus(Dictionary<RequestStatuses, int> statusCounts)
        {
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