// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Base
//  Layer:          SharedKernel
//  File:           DomainEvent.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Abstract domain event base for the Base layer.
//                  Concrete domain events MUST be sealed records and MUST NOT
//                  be mutated after construction. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Abstractions;

namespace PlantSafety.SharedKernel.Base;

/// <summary>
/// Abstract domain event base class.
/// Concrete domain events MUST be <c>sealed record</c> types with positional parameters.
/// </summary>
public abstract record DomainEvent : Abstractions.IDomainEvent
{
    /// <inheritdoc/>
    public abstract string EventType { get; }

    /// <inheritdoc/>
    public int EventVersion { get; init; } = 1;

    /// <inheritdoc/>
    public string SchemaVersion { get; init; } = "2026.1";

    /// <inheritdoc/>
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}
