﻿using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.Core;
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
            IAgentGuidParser agentGuidParser
            )
        {
            _processors = new List<ISyncProcessor>
            {
                new MovieProcessor(plexService, mediaItemProcessor),
                new TvProcessor(plexApi, plexService, mediaItemProcessor, agentGuidParser)
            };
        }

        public ISyncProcessor GetProcessor(string type)
        {
            var processor = _processors.FirstOrDefault(x => x.Type.ToString().Equals(type, StringComparison.InvariantCultureIgnoreCase));

            return processor;
        }
    }
}
