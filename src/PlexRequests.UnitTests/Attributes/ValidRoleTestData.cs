using System.Collections;
using System.Collections.Generic;
using PlexRequests.Core.Auth;

namespace PlexRequests.UnitTests.Attributes
{
    public class SuccessRoleTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new List<string> { PlexRequestRoles.Admin } };
            yield return new object[] { new List<string> { PlexRequestRoles.User } };
            yield return new object[] { new List<string> { PlexRequestRoles.Commenter } };
            yield return new object[] { new List<string> { PlexRequestRoles.Admin, PlexRequestRoles.User, PlexRequestRoles.Commenter } };
            yield return new object[] { new List<string>() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class FailRoleTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new List<string> { "" } };
            yield return new object[] { new List<string> { "admin" } };
            yield return new object[] { new List<string> { PlexRequestRoles.Admin, PlexRequestRoles.Admin, PlexRequestRoles.Commenter } };
            yield return new object[] { new List<string> { PlexRequestRoles.Admin.ToLower(), PlexRequestRoles.User.ToLower(), PlexRequestRoles.Commenter.ToLower() } };
            yield return new object[] { new List<string> { "invalid" } };
            yield return new object[] { new List<string> { "abc", "def" } };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}