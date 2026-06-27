// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Identifiers
//  Layer:          SharedKernel
//  File:           IdGenerator.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical factory for generating platform identifiers.
//                  All IDs MUST use UUIDv7 (time-ordered) for SQL clustering
//                  efficiency and distributed tracing correlation.
//                  NEVER use Guid.NewGuid() — always use this class or Uuid7.New().
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Identifiers;

/// <summary>
/// Canonical factory for platform identifiers.
/// All ID generation MUST go through this class or <see cref="Uuid7.New()"/>.
/// <c>Guid.NewGuid()</c> is PROHIBITED; it produces random v4 GUIDs that
/// fragment SQL clustered indexes.
/// </summary>
public static class IdGenerator
{
    /// <summary>Generates a new time-ordered UUIDv7 Guid.</summary>
    public static Guid NewId() => Uuid7.New();

    /// <summary>Generates a new <see cref="CorrelationId"/> for distributed tracing.</summary>
    public static CorrelationId NewCorrelationId() => new(Uuid7.New());

    /// <summary>Generates a new <see cref="CausationId"/> for command causation tracking.</summary>
    public static CausationId NewCausationId() => new(Uuid7.New());

    /// <summary>Generates a new <see cref="UserId"/>.</summary>
    public static UserId NewUserId() => new(Uuid7.New());

    /// <summary>
    /// Generates a deterministic correlation id from a known seed for use in tests.
    /// MUST only be called from test assemblies.
    /// </summary>
    public static CorrelationId DeterministicCorrelationId(Guid seed) => new(seed);
}
