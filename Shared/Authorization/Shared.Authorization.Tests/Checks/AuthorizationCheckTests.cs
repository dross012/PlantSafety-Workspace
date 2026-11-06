// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           AuthorizationCheckTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Checks;

public sealed class AuthorizationCheckTests
{
    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();

    [Fact]
    public void Evaluate_PlatformAdmin_AllowsEveryVerb()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

        foreach (ModulePermissionVerb verb in Enum.GetValues<ModulePermissionVerb>())
        {
            AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", verb).IsAuthorized.Should().BeTrue($"PlatformAdmin should be granted {verb}");
        }
    }

    [Fact]
    public void Evaluate_Viewer_AllowsReadOnly()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");

        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Read).IsAuthorized.Should().BeTrue();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Write).IsAuthorized.Should().BeFalse();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Approve).IsAuthorized.Should().BeFalse();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Admin).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_Editor_AllowsReadAndWriteOnly()
    {
        var caller = new CallerContext(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-3");

        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Read).IsAuthorized.Should().BeTrue();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Write).IsAuthorized.Should().BeTrue();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Approve).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_DeniedDecision_HasReasonExplainingDenial()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-4");

        AuthorizationDecision decision = AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Admin);

        decision.IsAuthorized.Should().BeFalse();
        decision.Reason.Should().Contain("Safety.Viewer").And.Contain("TestModule.Admin");
    }

    [Fact]
    public void EvaluateWithMfa_AuthorizedButMfaNotVerified_Denies()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotVerified, "corr-5");

        AuthorizationDecision decision = AuthorizationCheck.EvaluateWithMfa(caller, Resolver, "TestModule", ModulePermissionVerb.Read);

        decision.IsAuthorized.Should().BeFalse();
        decision.Reason.Should().Contain("MFA required");
    }

    [Fact]
    public void EvaluateWithMfa_AuthorizedAndMfaVerified_Allows()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.Verified, "corr-6");

        AuthorizationCheck.EvaluateWithMfa(caller, Resolver, "TestModule", ModulePermissionVerb.Read).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void EvaluateWithMfa_NotAuthorizedRegardlessOfMfa_Denies()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.Verified, "corr-7");

        AuthorizationDecision decision = AuthorizationCheck.EvaluateWithMfa(caller, Resolver, "TestModule", ModulePermissionVerb.Admin);

        decision.IsAuthorized.Should().BeFalse();
        decision.Reason.Should().NotContain("MFA");
    }
}
