// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions.Random
//  File:           IRandomProvider.cs
//  Created:        2026-08-09
//  Description:    Phase 2.1: the one approved seam for randomization
//                  (including id generation via NewGuid). Call sites that
//                  need a random value or a new identity take an
//                  IRandomProvider via constructor injection instead of
//                  calling Random.Shared/Guid.NewGuid() directly, so
//                  randomized behavior can be pinned to a fixed seed
//                  under DeterministicMode.
// ============================================================================
namespace Shared.Deterministic.Abstractions.Random;

/// <summary>The one approved seam for randomization and id generation.</summary>
public interface IRandomProvider
{
    /// <summary>Returns a random integer in [minInclusive, maxExclusive).</summary>
    int Next(int minInclusive, int maxExclusive);

    /// <summary>Returns a random double in [0.0, 1.0).</summary>
    double NextDouble();

    /// <summary>Returns a new identity value.</summary>
    Guid NewGuid();
}
