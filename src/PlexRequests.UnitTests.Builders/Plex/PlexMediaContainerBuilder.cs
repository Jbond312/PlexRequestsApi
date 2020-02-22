using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class PlexMediaContainerBuilder : IBuilder<PlexMediaContainer>
    {
        private List<MetadataBuilder> _metadatas;
        private List<DirectoryBuilder> _directories;
        private string _key;

        public PlexMediaContainerBuilder()
        {
            _key = Guid.NewGuid().ToString();
            _metadatas = new List<MetadataBuilder>();
            _directories = new List<DirectoryBuilder>();
        }

        public PlexMediaContainerBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public PlexMediaContainerBuilder WithMetadata(MetadataBuilder metadataBuilder)
        {
            _metadatas.Add(metadataBuilder);
            return this;
        }

        public PlexMediaContainerBuilder WithMetadata(int metadataCount = 1)
        {
            for (var i = 0; i < metadataCount; i++)
            {
                _metadatas.Add(new MetadataBuilder().WithIndex(i));
            }
            return this;
        }

        public PlexMediaContainerBuilder WithDirectory(DirectoryBuilder directoryBuilder)
        {
            _directories.Add(directoryBuilder);
            return this;
        }

        public PlexMediaContainerBuilder WithDirectories(int directoryCount = 1)
        {
            for (var i = 0; i < directoryCount; i++)
            {
                _directories.Add(new DirectoryBuilder());
            }
            return this;
        }

        public PlexMediaContainer Build()
        {
            return new PlexMediaContainer
            {
                MediaContainer = new MediaContainer
                {
                    Key = _key,
                    Metadata = _metadatas.Select(x => x.Build()).ToList(),
                    Directory = _directories.Select(x => x.Build()).ToList()
                }
            };
        }
    }
}
