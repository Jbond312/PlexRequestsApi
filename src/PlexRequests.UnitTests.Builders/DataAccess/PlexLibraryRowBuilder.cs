using System;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class PlexLibraryRowBuilder : IBuilder<PlexLibraryRow>
    {
        private int _plexLibraryId;
        private string _libraryKey;
        private string _type;
        private string _title;
        private bool _isEnabled;
        private bool _isArchived;

        public PlexLibraryRowBuilder()
        {
            _plexLibraryId = 1;
            _libraryKey = Guid.NewGuid().ToString();
            _type = Guid.NewGuid().ToString();
            _title = Guid.NewGuid().ToString();
            _isEnabled = true;
            _isArchived = false;
        }

        public PlexLibraryRowBuilder WithLibraryKey(string libraryKey)
        {
            _libraryKey = libraryKey;
            return this;
        }

        public PlexLibraryRowBuilder WithIsEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
            return this;
        }

        public PlexLibraryRowBuilder WithIsArchived(bool isArchived)
        {
            _isArchived = isArchived;
            return this;
        }

        public PlexLibraryRowBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public PlexLibraryRow Build()
        {
            return new PlexLibraryRow
            {
                PlexLibraryId = _plexLibraryId,
                LibraryKey = _libraryKey,
                Type = _type,
                Title = _title,
                IsEnabled = _isEnabled,
                IsArchived = _isArchived
            };
        }
    }
}
