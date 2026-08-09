// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.ValueObjects
//  Layer:          Abstractions
//  File:           EmployeeId.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed identifier for an Employee aggregate.
//                  Phase C.1 (Path C: Operational Modernization) -- the
//                  first real Employee identity in the PlantSafety codebase.
// ============================================================================
namespace Shared.EmployeeDomain.Abstractions.ValueObjects;

/// <summary>Strongly typed identifier for an <c>EmployeeAggregate</c>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct EmployeeId
{
    private EmployeeId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="EmployeeId"/>.</summary>
    public static EmployeeId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as an <see cref="EmployeeId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static EmployeeId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("EmployeeId cannot be Guid.Empty.", nameof(value));
        }

        return new EmployeeId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
