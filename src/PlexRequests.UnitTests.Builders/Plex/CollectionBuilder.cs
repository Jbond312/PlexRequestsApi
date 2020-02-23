using System;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class CollectionBuilder : IBuilder<Collection>
    {
        private string _tag;

        public CollectionBuilder()
        {
            _tag = Guid.NewGuid().ToString();
        }

        public CollectionBuilder WithTag(string tag)
        {
            _tag = tag;
            return this;
        }

        public Collection Build()
        {
            return new Collection
            {
                Tag = _tag
            };
        }
    }
}
