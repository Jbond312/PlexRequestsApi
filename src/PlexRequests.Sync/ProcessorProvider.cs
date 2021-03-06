﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Settings;
using PlexRequests.Plex;
using PlexRequests.Sync.SyncProcessors;

namespace PlexRequests.Sync
{
    public class ProcessorProvider : IProcessorProvider
    {
        private readonly List<ISyncProcessor> _processors;

        public ProcessorProvider(
            IPlexApi plexApi,
            IPlexService plexService,
            IMediaItemProcessor mediaItemProcessor,
            IAgentGuidParser agentGuidParser,
            IOptionsSnapshot<PlexSettings> plexSettings,
            ILoggerFactory loggerFactory
            )
        {
            _processors = new List<ISyncProcessor>
            {
                new MovieProcessor(plexService, mediaItemProcessor, plexSettings.Value, loggerFactory),
                new TvProcessor(plexApi, plexService, mediaItemProcessor, plexSettings.Value, agentGuidParser, loggerFactory)
            };
        }

        public ISyncProcessor GetProcessor(string type)
        {
            var processor = _processors.FirstOrDefault(x => x.Type.ToString().Equals(type, StringComparison.InvariantCultureIgnoreCase));

            return processor;
        }
    }
}
