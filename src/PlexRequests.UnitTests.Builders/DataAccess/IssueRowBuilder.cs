using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class IssueRowBuilder : IBuilder<IssueRow>
    {
        private int _issueId;
        private string _title;
        private string _description;
        private IssueStatuses _issueStatus;
        private MoviePlexMediaItemRowBuilder _mediaItemBuilder;
        private UserRowBuilder _userBuilder;
        private List<IssueCommentRowBuilder> _issueCommentBuilders;

        public IssueRowBuilder()
        {
            _issueId = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _description = Guid.NewGuid().ToString();
            _issueStatus = IssueStatuses.Pending;
            _mediaItemBuilder = new MoviePlexMediaItemRowBuilder();
            _userBuilder = new UserRowBuilder();
            _issueCommentBuilders = new List<IssueCommentRowBuilder>();
        }

        public IssueRowBuilder WithId(int id)
        {
            _issueId = id;
            return this;
        }

        public IssueRowBuilder WithUserId(int userId)
        {
            _userBuilder.WithUserId(userId);
            return this;
        }

        public IssueRowBuilder WithIssueStatus(IssueStatuses issueStatus)
        {
            _issueStatus = issueStatus;
            return this;
        }

        public IssueRowBuilder WithComments(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                _issueCommentBuilders.Add(new IssueCommentRowBuilder().WithId(i));
            }

            return this;
        }

        public IssueRowBuilder WithComment(IssueCommentRowBuilder issueCommentBuilder)
        {
            _issueCommentBuilders.Add(issueCommentBuilder);
            return this;
        }

        public IssueRow Build()
        {
            var user = _userBuilder.Build();
            var mediaItem = _mediaItemBuilder.Build();
            return new IssueRow
            {
                IssueId = _issueId,
                Title = _title,
                Description = _description,
                IssueStatus = _issueStatus,
                PlexMediaItem = mediaItem,
                PlexMediaItemId = mediaItem.PlexMediaItemId,
                User = user,
                UserId = user.UserId,
                IssueComments = _issueCommentBuilders.Select(x => x.Build()).ToList()
            };
        }
    }
}
