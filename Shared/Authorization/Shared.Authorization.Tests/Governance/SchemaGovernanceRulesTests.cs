// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           SchemaGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class SchemaGovernanceRulesTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly CallerContext Caller = new(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public void ValidateAll_AllValid_ReturnsNoViolations()
    {
        var callerResolution = new CallerContextResolution(Caller, ApiKeyIdentity.NoKeyProvided(EnterpriseRole.PlatformAdmin));
        var tenantContext = new TenantContext("acme-plant-1", null, Caller, "corr-1");
        var tenantResolution = new TenantContextResolution(tenantContext, false, false);
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Now);

        SchemaGovernanceRules.ValidateAll(callerResolution, tenantResolution, metadata).Should().BeEmpty();
    }

    [Fact]
    public void ValidateAll_AllNull_ReturnsViolationsFromEveryRuleSet()
    {
        IReadOnlyList<string> violations = SchemaGovernanceRules.ValidateAll(null, null, null);

        violations.Should().Contain("MissingResolution");
        violations.Should().Contain("MissingTenantResolution");
        violations.Should().Contain("MissingContractMetadata");
    }
}
