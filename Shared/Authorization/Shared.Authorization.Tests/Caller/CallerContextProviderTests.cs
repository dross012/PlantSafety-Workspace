// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           CallerContextProviderTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;

using Xunit;

namespace Shared.Authorization.Tests.Caller;

public sealed class CallerContextProviderTests
{
    [Fact]
    public void Current_BeforeSet_Throws()
    {
        var provider = new CallerContextProvider();

        Action act = () => _ = provider.Current;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Current_AfterSet_ReturnsSetValue()
    {
        var provider = new CallerContextProvider();
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

        provider.Set(caller);

        provider.Current.Should().Be(caller);
    }

    [Fact]
    public void Set_Null_Throws()
    {
        var provider = new CallerContextProvider();

        Action act = () => provider.Set(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
