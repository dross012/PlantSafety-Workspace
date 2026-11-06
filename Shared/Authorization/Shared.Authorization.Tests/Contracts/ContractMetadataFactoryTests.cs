// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           ContractMetadataFactoryTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Contracts;
using Shared.Deterministic.Abstractions.Clock;

using Xunit;

namespace Shared.Authorization.Tests.Contracts;

public sealed class ContractMetadataFactoryTests
{
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void Create_ComposesFieldsFromTenantContextAndClock()
    {
        var caller = new CallerContext(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");
        var tenantContext = new TenantContext("acme-plant-1", null, caller, "corr-1");

        ContractMetadata metadata = ContractMetadataFactory.Create(tenantContext, Clock);

        metadata.CallerRole.Should().Be("Safety.Editor");
        metadata.CorrelationId.Should().Be("corr-1");
        metadata.MfaStatus.Should().Be(MfaStatus.NotApplicable);
        metadata.TenantId.Should().Be("acme-plant-1");
        metadata.TimestampUtc.Should().Be(Clock.UtcNow);
    }

    [Fact]
    public void Create_UnscopedTenant_PropagatesSentinelTenantId()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-2");
        TenantContext tenantContext = TenantContext.Unscoped(caller, "corr-2");

        ContractMetadata metadata = ContractMetadataFactory.Create(tenantContext, Clock);

        metadata.TenantId.Should().Be(TenantContext.UnscopedTenantId);
    }

    [Fact]
    public void Create_NullTenantContext_Throws()
    {
        Action act = () => ContractMetadataFactory.Create(null!, Clock);
        act.Should().Throw<ArgumentNullException>();
    }
}
