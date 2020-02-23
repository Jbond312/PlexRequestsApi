using System;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class IssueCommentRowBuilder : IBuilder<IssueCommentRow>
    {
        private int _issueCommentId;
        private string _comment;
        private int _likeCount;
        private UserRowBuilder _userBuilder;

        public IssueCommentRowBuilder()
        {
            _issueCommentId = new Random().Next(1, int.MaxValue);
            _comment = Guid.NewGuid().ToString();
            _likeCount = new Random().Next(0, int.MaxValue);
            _userBuilder = new UserRowBuilder();
        }

        public IssueCommentRowBuilder WithId(int id)
        {
            _issueCommentId = id;
            return this;
        }

        public IssueCommentRowBuilder WithUserId(int userId)
        {
            _userBuilder.WithUserId(userId);
            return this;
        }

        public IssueCommentRow Build()
        {
            var user = _userBuilder.Build();
            return new IssueCommentRow
            {
                IssueCommentId = _issueCommentId,
                Comment = _comment,
                LikeCount = _likeCount,
                User = user,
                UserId = user.UserId
            };
        }
    }
}
