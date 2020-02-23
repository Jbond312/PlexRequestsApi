using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Helpers
{
    public class RequestHelper : IRequestHelper
    {
        private IEnumerable<RequestStatuses> _allStatuses = Enum.GetValues(typeof(RequestStatuses)).Cast<RequestStatuses>();

        public void SetAggregatedStatus(TvRequestRow request)
        {
            if (request.Track)
            {
                return;
            }

            var allStatusCounts = _allStatuses.ToDictionary(status => status, count => 0);

            foreach (var season in request.TvRequestSeasons)
            {
                var seasonStatusCounts = _allStatuses.ToDictionary(status => status, count => 0);

                foreach (var episode in season.TvRequestEpisodes)
                {
                    allStatusCounts[episode.RequestStatus]++;
                    seasonStatusCounts[episode.RequestStatus]++;
                }

                season.RequestStatus = CalculateStatus(seasonStatusCounts);
            }

            request.RequestStatus = CalculateStatus(allStatusCounts);
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