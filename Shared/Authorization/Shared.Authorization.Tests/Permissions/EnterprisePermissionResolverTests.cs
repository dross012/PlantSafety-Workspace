// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           EnterprisePermissionResolverTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Permissions;

public sealed class EnterprisePermissionResolverTests
{
    private static readonly EnterprisePermissionResolver Resolver = new();

    [Fact]
    public void Resolve_PlatformAdmin_GrantsAllThreeVerbs()
    {
        IReadOnlySet<string> granted = Resolver.Resolve(EnterpriseRole.PlatformAdmin, "TestModule");

        granted.Should().Contain("TestModule.Read");
        granted.Should().Contain("TestModule.Write");
        granted.Should().Contain("TestModule.Admin");
    }

    [Fact]
    public void Resolve_Viewer_GrantsOnlyRead()
    {
        IReadOnlySet<string> granted = Resolver.Resolve(EnterpriseRole.Viewer, "TestModule");

        granted.Should().ContainSingle().Which.Should().Be("TestModule.Read");
    }

    [Fact]
    public void Resolve_Operator_GrantsReadAndWriteNotAdmin()
    {
        IReadOnlySet<string> granted = Resolver.Resolve(EnterpriseRole.Operator, "TestModule");

        granted.Should().Contain("TestModule.Read");
        granted.Should().Contain("TestModule.Write");
        granted.Should().NotContain("TestModule.Admin");
    }

    [Fact]
    public void Resolve_UnrecognizedRole_ReturnsEmptySet()
    {
        IReadOnlySet<string> granted = Resolver.Resolve((EnterpriseRole)999, "TestModule");

        granted.Should().BeEmpty();
    }

    [Fact]
    public void ResolveAcrossModules_UnionsPermissionsFromEveryModule()
    {
        IReadOnlySet<string> granted = Resolver.ResolveAcrossModules(EnterpriseRole.Viewer, new[] { "ModuleA", "ModuleB" });

        granted.Should().Contain("ModuleA.Read");
        granted.Should().Contain("ModuleB.Read");
    }
}
