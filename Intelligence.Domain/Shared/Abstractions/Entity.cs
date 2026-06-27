/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               Entity.cs
    DESCRIPTION:        Base class for all Entities in the Intelligence domain. Equality is determined by
                        identity (Id), not by field values. Entities that are aggregate roots should extend
                        AggregateRoot<TId> instead. (Rule 2.5)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.Shared.Abstractions;

/// <summary>
/// Base class for Intelligence domain Entities. Two instances with the same <typeparamref name="TId"/>
/// are considered equal regardless of other field values.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier. Must be a struct.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : struct
{
    /// <summary>The identity of this entity.</summary>
    public TId Id { get; protected init; }

    protected Entity(TId id) => Id = id;

    /// <inheritdoc />
    public bool Equals(Entity<TId>? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);
}
