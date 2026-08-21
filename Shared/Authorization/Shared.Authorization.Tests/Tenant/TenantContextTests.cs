// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantContextTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;

using Xunit;

namespace Shared.Authorization.Tests.Tenant;

public sealed class TenantContextTests
{
    [Fact]
    public void Unscoped_ReturnsSentinelTenantIdAndNullFacilityId()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-1");

        TenantContext context = TenantContext.Unscoped(caller, "corr-1");

        context.TenantId.Should().Be(TenantContext.UnscopedTenantId);
        context.FacilityId.Should().BeNull();
        context.Caller.Should().Be(caller);
        context.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void SchemaVersion_IsPositive()
    {
        TenantContext.SchemaVersion.Should().BeGreaterThan(0);
    }
}
