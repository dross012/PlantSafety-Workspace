// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions.Random
//  File:           SystemRandomProvider.cs
//  Created:        2026-08-09
//  Description:    The production IRandomProvider: a thin wrapper over
//                  System.Random.Shared and Guid.NewGuid(). The only place
//                  either is called directly -- every other call site
//                  should depend on IRandomProvider instead.
// ============================================================================
namespace Shared.Deterministic.Abstractions.Random;

/// <summary>The production random provider: true, non-deterministic randomness.</summary>
public sealed class SystemRandomProvider : IRandomProvider
{
    /// <inheritdoc />
    public int Next(int minInclusive, int maxExclusive) => System.Random.Shared.Next(minInclusive, maxExclusive);

    /// <inheritdoc />
    public double NextDouble() => System.Random.Shared.NextDouble();

    /// <inheritdoc />
    public Guid NewGuid() => Guid.NewGuid();
}
