using System.Collections.Generic;
using PlexRequests.DataAccess;

namespace PlexRequests.UnitTests.Builders
{
    public static class BuilderExtensions
    {
        public static List<T> CreateMany<T>(this IBuilder<T> builder, int amountToCreate = 3) where T : class
        {
            var entities = new List<T>();

            for (var i = 0; i < amountToCreate; i++)
            {
                entities.Add(builder.Build());
            }

            return entities;
        }

        public static Paged<T> CreatePaged<T>(this IBuilder<T> builder, int amountToCreate = 3) where T : class
        {
            var entities = new List<T>();

            for (var i = 0; i < amountToCreate; i++)
            {
                entities.Add(builder.Build());
            }

            return new Paged<T>
            {
                Items = entities,
                Page = 1,
                PageSize = 1,
                TotalPages = 1
            };
        }
    }
}
