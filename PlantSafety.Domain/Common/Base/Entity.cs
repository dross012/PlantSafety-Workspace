// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           Entity.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Abstract base class for domain entities. Equality is determined by
//      identity (Id), not by value. Implements structural equality operators.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Base;

/// <summary>Abstract base for all domain entities. Equality is identity-based.</summary>
public abstract class Entity<TId> : IEntity<TId>
{
    /// <inheritdoc/>
    public TId Id { get; protected set; } = default!;

    /// <summary>For ORM/rehydration only.</summary>
    protected Entity() { }

    /// <summary>Initialises with explicit identity.</summary>
    protected Entity(TId id) => Id = id;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id != null && Id.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Id?.GetHashCode() ?? 0;

    /// <summary>Reference equality via <see cref="Equals(object?)"/>.</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Reference inequality via <see cref="Equals(object?)"/>.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
