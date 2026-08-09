// ============================================================================
//  Project:        Shared.Deterministic.Tests
//  File:           SeededRandomProviderTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Random;

using Xunit;

namespace Shared.Deterministic.Tests.Random;

public sealed class SeededRandomProviderTests
{
    [Fact]
    public void Next_SameSeedAndCallOrder_ProducesTheSameSequence()
    {
        var providerA = new SeededRandomProvider(42);
        var providerB = new SeededRandomProvider(42);

        int[] sequenceA = [providerA.Next(0, 1000), providerA.Next(0, 1000), providerA.Next(0, 1000)];
        int[] sequenceB = [providerB.Next(0, 1000), providerB.Next(0, 1000), providerB.Next(0, 1000)];

        sequenceA.Should().BeEquivalentTo(sequenceB, options => options.WithStrictOrdering());
    }

    [Fact]
    public void NewGuid_SameSeedAndCallOrder_ProducesTheSameGuid()
    {
        var providerA = new SeededRandomProvider(7);
        var providerB = new SeededRandomProvider(7);

        Guid guidA = providerA.NewGuid();
        Guid guidB = providerB.NewGuid();

        guidA.Should().Be(guidB);
    }

    [Fact]
    public void Next_DifferentSeeds_ProduceDifferentSequences()
    {
        var providerA = new SeededRandomProvider(1);
        var providerB = new SeededRandomProvider(2);

        int[] sequenceA = [providerA.Next(0, int.MaxValue), providerA.Next(0, int.MaxValue), providerA.Next(0, int.MaxValue)];
        int[] sequenceB = [providerB.Next(0, int.MaxValue), providerB.Next(0, int.MaxValue), providerB.Next(0, int.MaxValue)];

        sequenceA.Should().NotBeEquivalentTo(sequenceB);
    }

    [Fact]
    public void NextDouble_AlwaysWithinZeroToOne()
    {
        var provider = new SeededRandomProvider(99);

        for (var i = 0; i < 50; i++)
        {
            double value = provider.NextDouble();
            value.Should().BeGreaterThanOrEqualTo(0.0);
            value.Should().BeLessThan(1.0);
        }
    }
}
