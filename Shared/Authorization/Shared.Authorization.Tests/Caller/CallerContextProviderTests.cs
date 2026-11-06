// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           CallerContextProviderTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

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
    public void Current_AfterSet_ReturnsTheSetCallerContext()
    {
        var provider = new CallerContextProvider();
        var caller = new CallerContext(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

        provider.Set(caller);

        provider.Current.Should().Be(caller);
    }
}
