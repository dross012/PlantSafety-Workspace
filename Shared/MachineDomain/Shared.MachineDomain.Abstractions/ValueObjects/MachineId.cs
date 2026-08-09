// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.ValueObjects
//  File:           MachineId.cs
//  Created:        2026-08-08
//  Description:    Strongly typed identifier for a Machine aggregate.
//                  Phase C.2 (Path C: Operational Modernization) -- the
//                  first real Machine/Equipment identity in the PlantSafety
//                  codebase. Mirrors Shared.FacilityDomain.FacilityId /
//                  Shared.EmployeeDomain.EmployeeId.
// ============================================================================
namespace Shared.MachineDomain.Abstractions.ValueObjects;

/// <summary>Strongly typed identifier for a <c>MachineAggregate</c>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct MachineId
{
    private MachineId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="MachineId"/>.</summary>
    public static MachineId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as a <see cref="MachineId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static MachineId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MachineId cannot be Guid.Empty.", nameof(value));
        }

        return new MachineId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
