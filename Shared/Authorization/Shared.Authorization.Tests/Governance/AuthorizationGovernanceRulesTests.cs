// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           AuthorizationGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Infrastructure.Governance;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class AuthorizationGovernanceRulesTests
{
    [Fact]
    public void ValidateCallerContext_Null_ReturnsViolation()
    {
        AuthorizationGovernanceRules.ValidateCallerContext(null).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateCallerContext_ValidContext_ReturnsNoViolations()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-1");

        AuthorizationGovernanceRules.ValidateCallerContext(caller).Should().BeEmpty();
    }

    [Fact]
    public void ValidateCallerContext_EmptyCorrelationId_ReturnsViolation()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "");

        AuthorizationGovernanceRules.ValidateCallerContext(caller).Should().Contain(v => v.Contains("CorrelationId"));
    }

    [Fact]
    public void ValidatePermissionShape_EmptyModuleOrVerb_ReturnsViolations()
    {
        AuthorizationGovernanceRules.ValidatePermissionShape("", "Read").Should().NotBeEmpty();
        AuthorizationGovernanceRules.ValidatePermissionShape("Facility", "").Should().NotBeEmpty();
        AuthorizationGovernanceRules.ValidatePermissionShape("Facility", "Read").Should().BeEmpty();
    }

    [Fact]
    public void ValidateRoleInheritanceConsistency_MatchesRankComparison_ReturnsNoViolations()
    {
        AuthorizationGovernanceRules.ValidateRoleInheritanceConsistency(EnterpriseRole.PlatformAdmin, EnterpriseRole.Viewer).Should().BeEmpty();
        AuthorizationGovernanceRules.ValidateRoleInheritanceConsistency(EnterpriseRole.Viewer, EnterpriseRole.PlatformAdmin).Should().BeEmpty();
    }
}
