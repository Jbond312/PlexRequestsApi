using System.Collections.Generic;
using FluentAssertions;
using PlexRequests.Functions.Attributes;
using Xunit;

namespace PlexRequests.UnitTests.Attributes
{
    public class ValidRoleAttributeTests
    {
        [Theory]
        [ClassData(typeof(FailRoleTestData))]
        public void Is_Invalid_When_Invalid_Roles(List<string> roles)
        {
            var underTest = new ValidRoleAttribute();
            var result = underTest.IsValid(roles);

            result.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(SuccessRoleTestData))]
        public void Is_Valid_When_Vvalid_Roles(List<string> roles)
        {
            var underTest = new ValidRoleAttribute();
            var result = underTest.IsValid(roles);

            result.Should().BeTrue();
        }
    }
}