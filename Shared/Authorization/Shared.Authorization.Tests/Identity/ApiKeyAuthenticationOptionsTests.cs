// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           ApiKeyAuthenticationOptionsTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Identity;

public sealed class ApiKeyAuthenticationOptionsTests
{
    [Fact]
    public void ToRoleMap_DefaultOptions_ReturnsEmptyMap()
    {
        new ApiKeyAuthenticationOptions().ToRoleMap().Should().BeEmpty();
    }

    [Fact]
    public void ToRoleMap_ValidRoleNames_ResolvesEnterpriseRoles()
    {
        var options = new ApiKeyAuthenticationOptions
        {
            RegisteredKeys = new Dictionary<string, string>
            {
                ["key-1"] = "Safety.Editor",
                ["key-2"] = "Platform.Admin",
            },
        };

        IReadOnlyDictionary<string, EnterpriseRole> map = options.ToRoleMap();

        map.Should().HaveCount(2);
        map["key-1"].Should().Be(EnterpriseRole.Editor);
        map["key-2"].Should().Be(EnterpriseRole.PlatformAdmin);
    }

    [Fact]
    public void ToRoleMap_UnknownRoleName_DropsEntryWithoutThrowing()
    {
        var options = new ApiKeyAuthenticationOptions
        {
            RegisteredKeys = new Dictionary<string, string>
            {
                ["key-1"] = "Not.A.Real.Role",
                ["key-2"] = "Safety.Viewer",
            },
        };

        IReadOnlyDictionary<string, EnterpriseRole> map = options.ToRoleMap();

        map.Should().HaveCount(1);
        map.Should().ContainKey("key-2");
    }
}
