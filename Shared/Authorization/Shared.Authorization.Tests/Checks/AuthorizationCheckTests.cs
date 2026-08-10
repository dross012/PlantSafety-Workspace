// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           AuthorizationCheckTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

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
    public void Evaluate_PlatformAdmin_GrantedReadWriteAdminWhenMfaVerified()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.Verified, "corr-1");

        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Read).IsAuthorized.Should().BeTrue();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Write).IsAuthorized.Should().BeTrue();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Admin).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_Viewer_GrantedReadOnly()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");

        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Read).IsAuthorized.Should().BeTrue();
        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Write).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_AdminVerbWithoutMfaVerified_DeniesWithMfaRequirementFailed()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.Required, "corr-3");

        AuthorizationDecision decision = AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Admin);

        decision.IsAuthorized.Should().BeFalse();
        decision.MfaRequirementFailed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_AdminVerbWithMfaVerified_Allows()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.Verified, "corr-4");

        AuthorizationCheck.Evaluate(caller, Resolver, "TestModule", ModulePermissionVerb.Admin).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_NullCaller_Throws()
    {
        Action act = () => AuthorizationCheck.Evaluate(null!, Resolver, "TestModule", ModulePermissionVerb.Read);

        act.Should().Throw<ArgumentNullException>();
    }
}
