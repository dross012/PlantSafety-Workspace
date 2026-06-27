/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ValueObject.cs
    DESCRIPTION:        Base class for all Value Objects in the Intelligence domain. Provides structural
                        equality via GetEqualityComponents(). Value Objects are immutable; all state must
                        be set in the constructor and never mutated. (Rule 2.8)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.Shared.Abstractions;

/// <summary>
/// Base class for Intelligence domain Value Objects. Equality is determined by value, not identity.
/// Derived classes must implement <see cref="GetEqualityComponents"/> to enumerate all fields
/// that participate in equality.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Returns the components that determine equality for this value object.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component));

    /// <summary>Equality operator.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !(left == right);
}
