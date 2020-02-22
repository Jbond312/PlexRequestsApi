using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class MediumBuilder : IBuilder<Medium>
    {
        private int _id;
        private string _videoResolution;

        public MediumBuilder()
        {
            _id = 1;
            _videoResolution = "1080p";
        }

        public MediumBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public MediumBuilder WithResolution(string resolution)
        {
            _videoResolution = resolution;
            return this;
        }

        public Medium Build()
        {
            return new Medium
            {
                Id = _id,
                VideoResolution = _videoResolution
            };
        }
    }
}
