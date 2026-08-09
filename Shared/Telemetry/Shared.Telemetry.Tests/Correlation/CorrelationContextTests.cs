// ============================================================================
//  Project:        Shared.Telemetry.Tests
//  File:           CorrelationContextTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Telemetry.Abstractions.Correlation;

using Xunit;

namespace Shared.Telemetry.Tests.Correlation;

public sealed class CorrelationContextTests
{
    [Fact]
    public void CorrelationId_BeforeSet_Throws()
    {
        var context = new CorrelationContext();

        Action act = () => _ = context.CorrelationId;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CorrelationId_AfterSet_ReturnsTheSetValue()
    {
        var context = new CorrelationContext();

        context.Set("corr-abc");

        context.CorrelationId.Should().Be("corr-abc");
    }
}
