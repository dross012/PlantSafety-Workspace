// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Base
//  Layer:          SharedKernel
//  File:           ValueObject.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Abstract value object base. Structural (attribute) equality
//                  determined by all properties. Immutable by convention.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Base;

/// <summary>
/// Abstract value object base. Equality is structural — two instances are equal
/// iff all their properties are equal.  Value objects MUST be immutable.
/// </summary>
public abstract class ValueObject
{
    /// <summary>Returns the ordered list of components that define equality.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(1, (hash, obj) => HashCode.Combine(hash, obj));

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
