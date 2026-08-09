// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions.Random
//  File:           SeededRandomProvider.cs
//  Created:        2026-08-09
//  Description:    A deterministic IRandomProvider backed by a seeded
//                  System.Random instance -- the same seed always produces
//                  the same sequence of Next/NextDouble/NewGuid results,
//                  in call order. Used by DeterministicMode integration
//                  tests. NewGuid() fills 16 bytes from the seeded
//                  generator rather than calling the real Guid.NewGuid(),
//                  so it is fully reproducible, not just "a" guid.
// ============================================================================
namespace Shared.Deterministic.Abstractions.Random;

/// <summary>A deterministic random provider: the same seed always reproduces the same sequence.</summary>
public sealed class SeededRandomProvider(int seed) : IRandomProvider
{
    private readonly System.Random _random = new(seed);

    /// <inheritdoc />
    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

    /// <inheritdoc />
    public double NextDouble() => _random.NextDouble();

    /// <inheritdoc />
    public Guid NewGuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        _random.NextBytes(bytes);
        return new Guid(bytes);
    }
}
