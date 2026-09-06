// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           RoleInheritanceResolverTests.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. RoleInheritanceResolver.cs existed
//                  on disk uncommitted before this phase (see the commit
//                  message capturing it into version control) with no test
//                  coverage of its own -- these tests cover it now.
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Roles;

public sealed class RoleInheritanceResolverTests
{
    [Fact]
    public void GetInheritedRoles_PlatformAdmin_ReturnsAllFourRolesLowestToHighest()
    {
        IReadOnlyList<EnterpriseRole> inherited = RoleInheritanceResolver.GetInheritedRoles(EnterpriseRole.PlatformAdmin);

        inherited.Should().Equal(EnterpriseRole.Viewer, EnterpriseRole.Editor, EnterpriseRole.Approver, EnterpriseRole.PlatformAdmin);
    }

    [Fact]
    public void GetInheritedRoles_Viewer_ReturnsOnlyViewer()
    {
        IReadOnlyList<EnterpriseRole> inherited = RoleInheritanceResolver.GetInheritedRoles(EnterpriseRole.Viewer);

        inherited.Should().Equal(EnterpriseRole.Viewer);
    }

    [Theory]
    [InlineData(0, 0, true)]   // Viewer satisfies Viewer
    [InlineData(3, 0, true)]   // PlatformAdmin satisfies Viewer
    [InlineData(0, 3, false)]  // Viewer does not satisfy PlatformAdmin
    [InlineData(1, 2, false)]  // Editor does not satisfy Approver
    [InlineData(2, 2, true)]   // Approver satisfies Approver
    public void Satisfies_MatchesRankComparison(int heldRank, int requiredRank, bool expected)
    {
        EnterpriseRole held = EnterpriseRole.All.Single(r => r.Rank == heldRank);
        EnterpriseRole required = EnterpriseRole.All.Single(r => r.Rank == requiredRank);

        RoleInheritanceResolver.Satisfies(held, required).Should().Be(expected);
    }

    [Fact]
    public void ResolveHighestRole_MultipleClaims_ReturnsHighestRank()
    {
        EnterpriseRole? result = RoleInheritanceResolver.ResolveHighestRole(["Safety.Viewer", "Safety.Approver", "Safety.Editor"]);

        result.Should().Be(EnterpriseRole.Approver);
    }

    [Fact]
    public void ResolveHighestRole_NoRecognizedClaims_ReturnsNull()
    {
        EnterpriseRole? result = RoleInheritanceResolver.ResolveHighestRole(["SomeLegacyRole", "AnotherOne"]);

        result.Should().BeNull();
    }
}
