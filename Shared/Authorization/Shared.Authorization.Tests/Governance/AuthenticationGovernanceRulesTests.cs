// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           AuthenticationGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Infrastructure.Governance;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class AuthenticationGovernanceRulesTests
{
    [Fact]
    public void ValidateResolution_Null_ReturnsMissingResolutionViolation()
    {
        AuthenticationGovernanceRules.ValidateResolution(null).Should().Contain("MissingResolution");
    }

    [Fact]
    public void ValidateResolution_NoKeyProvided_ReturnsNoViolations()
    {
        var caller = CallerContext.SystemDefault("corr-1");
        var resolution = new CallerContextResolution(caller, ApiKeyIdentity.NoKeyProvided(EnterpriseRole.PlatformAdmin));

        AuthenticationGovernanceRules.ValidateResolution(resolution).Should().BeEmpty();
    }

    [Fact]
    public void ValidateResolution_RecognizedKey_ReturnsNoViolations()
    {
        var caller = new CallerContext(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-2");
        var resolution = new CallerContextResolution(caller, ApiKeyIdentity.Recognized(EnterpriseRole.Editor));

        AuthenticationGovernanceRules.ValidateResolution(resolution).Should().BeEmpty();
    }

    [Fact]
    public void ValidateResolution_EmptyCorrelationId_ReturnsInvalidShapeViolation()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "");
        var resolution = new CallerContextResolution(caller, ApiKeyIdentity.NoKeyProvided(EnterpriseRole.PlatformAdmin));

        AuthenticationGovernanceRules.ValidateResolution(resolution).Should().Contain("InvalidCallerContextShape");
    }
}
