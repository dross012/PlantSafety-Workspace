// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class TenantGovernanceRulesTests
{
    private static readonly CallerContext Caller = new(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public void ValidateResolution_Null_ReturnsMissingResolutionViolation()
    {
        TenantGovernanceRules.ValidateResolution(null).Should().Contain("MissingTenantResolution");
    }

    [Fact]
    public void ValidateResolution_UnscopedNoKey_ReturnsNoViolations()
    {
        var context = TenantContext.Unscoped(Caller, "corr-1");
        var resolution = new TenantContextResolution(context, TenantKeyProvided: false, TenantRecognized: false);

        TenantGovernanceRules.ValidateResolution(resolution).Should().BeEmpty();
    }

    [Fact]
    public void ValidateResolution_RecognizedTenant_ReturnsNoViolations()
    {
        var context = new TenantContext("acme-plant-1", null, Caller, "corr-2");
        var resolution = new TenantContextResolution(context, TenantKeyProvided: true, TenantRecognized: true);

        TenantGovernanceRules.ValidateResolution(resolution).Should().BeEmpty();
    }

    [Fact]
    public void ValidateResolution_EmptyCorrelationId_ReturnsInvalidShapeViolation()
    {
        var context = new TenantContext("acme-plant-1", null, Caller, "");
        var resolution = new TenantContextResolution(context, TenantKeyProvided: false, TenantRecognized: false);

        TenantGovernanceRules.ValidateResolution(resolution).Should().Contain("InvalidTenantContextShape");
    }

    [Fact]
    public void ValidateTenantContext_Null_ReturnsViolation()
    {
        TenantGovernanceRules.ValidateTenantContext(null).Should().Contain("MissingTenantContext");
    }

    [Fact]
    public void ValidateTenantContext_ValidContext_ReturnsNoViolations()
    {
        TenantGovernanceRules.ValidateTenantContext(TenantContext.Unscoped(Caller, "corr-1")).Should().BeEmpty();
    }

    [Fact]
    public void ValidateIsolationDecisionShape_Null_ReturnsViolation()
    {
        TenantGovernanceRules.ValidateIsolationDecisionShape(null, Guid.NewGuid()).Should().Contain("MissingTenantAccessDecision");
    }

    [Fact]
    public void ValidateIsolationDecisionShape_DenialWithoutRequestedFacility_ReturnsViolation()
    {
        TenantAccessDecision decision = TenantAccessDecision.Deny("denied");

        TenantGovernanceRules.ValidateIsolationDecisionShape(decision, null).Should().Contain("DenialWithoutRequestedFacility");
    }

    [Fact]
    public void ValidateIsolationDecisionShape_ValidAllow_ReturnsNoViolations()
    {
        TenantAccessDecision decision = TenantAccessDecision.Allow("allowed");

        TenantGovernanceRules.ValidateIsolationDecisionShape(decision, null).Should().BeEmpty();
    }
}
