using System;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class DirectoryBuilder : IBuilder<Directory>
    {
        private string _key;
        private string _title;
        private string _type;
        private string _agent;
        private int _count;

        public DirectoryBuilder()
        {
            _key = Guid.NewGuid().ToString();
            _title = Guid.NewGuid().ToString();
            _type = Guid.NewGuid().ToString();
            _agent = Guid.NewGuid().ToString();
            _count = 1;
        }

        public Directory Build()
        {
            return new Directory
            {
                Key = _key,
                Title = _title,
                Type = _type,
                Agent = _agent,
                Count = _count
            };
        }
    }
}
