// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Guards
//  Layer:          SharedKernel
//  File:           Guard.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Top-level guard entry point. Forwards to the canonical
//                  Primitives.Guards.Guard. Provides a stable import path
//                  for callers that reference PlantSafety.SharedKernel.Guards.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Errors;
using PrimitivesGuard = PlantSafety.SharedKernel.Primitives.Guards.Guard;

namespace PlantSafety.SharedKernel.Guards;

/// <summary>
/// Top-level guard entry point. Delegates to <see cref="Primitives.Guards.Guard"/>.
/// Prefer the typed extension methods in <see cref="GuardExtensions"/> for
/// common invariant patterns.
/// </summary>
public static class Guard
{
    /// <inheritdoc cref="PrimitivesGuard.AgainstNull"/>
    public static void AgainstNull(object? value, string name)
        => PrimitivesGuard.AgainstNull(value, name);

    /// <inheritdoc cref="PrimitivesGuard.Against"/>
    public static void Against(bool condition, Error error)
        => PrimitivesGuard.Against(condition, error);
}
