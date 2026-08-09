// ============================================================================
//  Project:        Shared.Deterministic.Tests
//  File:           SystemRandomProviderTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Random;

using Xunit;

namespace Shared.Deterministic.Tests.Random;

public sealed class SystemRandomProviderTests
{
    [Fact]
    public void Next_ReturnsValueWithinRange()
    {
        IRandomProvider provider = new SystemRandomProvider();

        int value = provider.Next(10, 20);

        value.Should().BeGreaterThanOrEqualTo(10);
        value.Should().BeLessThan(20);
    }

    [Fact]
    public void NewGuid_ReturnsANonEmptyGuid()
    {
        IRandomProvider provider = new SystemRandomProvider();

        Guid guid = provider.NewGuid();

        guid.Should().NotBe(Guid.Empty);
    }
}
