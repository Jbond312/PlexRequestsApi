using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class MetadataBuilder : IBuilder<Metadata>
    {
        private string _ratingKey;
        private string _key;
        private string _guid;
        private string _type;
        private string _title;
        private int _year;
        private int _index;
        private string _grandParentRatingKey;
        private List<MediumBuilder> _mediums;
        private List<CollectionBuilder> _collections;

        public MetadataBuilder()
        {
            _ratingKey = new Random().Next(1, int.MaxValue).ToString();
            _key = Guid.NewGuid().ToString();
            _guid = Guid.NewGuid().ToString();
            _type = Guid.NewGuid().ToString();
            _title = Guid.NewGuid().ToString();
            _year = DateTime.UtcNow.Year;
            _index = 1;
            _grandParentRatingKey = new Random().Next(1, int.MaxValue).ToString();
            _mediums = new List<MediumBuilder>();
            _collections = new List<CollectionBuilder>();
        }

        public MetadataBuilder WithRatingKey(string ratingKey)
        {
            _ratingKey = ratingKey;
            return this;
        }

        public MetadataBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public MetadataBuilder WithGuid(string guid)
        {
            _guid = guid;
            return this;
        }

        public MetadataBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public MetadataBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public MetadataBuilder WithYear(int year)
        {
            _year = year;
            return this;
        }

        public MetadataBuilder WithIndex(int index)
        {
            _index = index;
            return this;
        }

        public MetadataBuilder WithGrandParentRatingKey(string grandParentRatingKey)
        {
            _grandParentRatingKey = grandParentRatingKey;
            return this;
        }

        public MetadataBuilder WithMedium(MediumBuilder mediumBuilder)
        {
            _mediums.Add(mediumBuilder);
            return this;
        }

        public MetadataBuilder WithMediums(int mediumCount = 1)
        {
            for (var i = 0; i < mediumCount; i++)
            {
                _mediums.Add(new MediumBuilder().WithId(i));
            }

            return this;
        }

        public MetadataBuilder WithCollection(CollectionBuilder collectionBuilder)
        {
            _collections.Add(collectionBuilder);
            return this;
        }

        public MetadataBuilder WithCollections(int collectionCount = 1)
        {
            for (var i = 0; i < collectionCount; i++)
            {
                _collections.Add(new CollectionBuilder());
            }

            return this;
        }

        public Metadata Build()
        {
            return new Metadata
            {
                RatingKey = _ratingKey,
                Key = _key,
                Guid = _guid,
                Type = _type,
                Title = _title,
                Year = _year,
                Index = _index,
                GrandParentRatingKey = _grandParentRatingKey,
                Media = _mediums.Select(x => x.Build()).ToList(),
                Collection = _collections.Select(x => x.Build()).ToList()
            };
        }
    }
}
