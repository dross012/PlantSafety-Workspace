// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Identifiers
//  Layer:          SharedKernel
//  File:           StronglyTypedId.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical readonly record struct base for module-specific
//                  StrongId value types. Wraps a Guid and rejects Guid.Empty.
//                  Use Uuid7.New() to generate values. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Identifiers;

/// <summary>
/// Canonical base struct for module-specific strong-typed identifiers.
/// <para>
/// Usage: Declare a module-specific id as:
/// <code>
/// public readonly record struct AlertId(Guid Value) : IStronglyTypedId
/// {
///     public AlertId() : this(Uuid7.New()) { }
///     public static AlertId New()           => new(Uuid7.New());
///     public static AlertId FromGuid(Guid g) => new(g);
///     public override string ToString()      => Value.ToString("D");
/// }
/// </code>
/// </para>
/// Additive-only evolution.
/// </summary>
public interface IStronglyTypedId
{
    /// <summary>Underlying Guid value. Must not be <see cref="Guid.Empty"/>.</summary>
    Guid Value { get; }
}
