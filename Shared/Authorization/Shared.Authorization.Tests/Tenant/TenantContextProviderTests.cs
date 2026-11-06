// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantContextProviderTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;

using Xunit;

namespace Shared.Authorization.Tests.Tenant;

public sealed class TenantContextProviderTests
{
    [Fact]
    public void Current_BeforeSet_Throws()
    {
        var provider = new TenantContextProvider();

        Action act = () => _ = provider.Current;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Current_AfterSet_ReturnsTheSetTenantContext()
    {
        var provider = new TenantContextProvider();
        var caller = new CallerContext(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");
        var context = new TenantContext("acme-plant-1", null, caller, "corr-1");

        provider.Set(context);

        provider.Current.Should().Be(context);
    }
}
