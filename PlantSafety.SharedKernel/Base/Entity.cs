// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Base
//  Layer:          SharedKernel
//  File:           Entity.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Abstract entity base. Identity-based equality; no attribute equality.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Base;

/// <summary>
/// Abstract entity base. Two entities are equal iff their Ids are equal.
/// </summary>
/// <typeparam name="TId">Strong-typed identity of this entity.</typeparam>
public abstract class Entity<TId> where TId : struct
{
    /// <summary>Strong-typed entity identity.</summary>
    public TId Id { get; protected set; }

    protected Entity() { }
    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return obj is Entity<TId> other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
