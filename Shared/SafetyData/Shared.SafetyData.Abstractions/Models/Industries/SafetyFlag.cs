// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           SafetyFlag.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Canonical set of safety/compliance flags applicable to an
//                  IndustryDefinition. Bitwise-combinable so a single industry
//                  can carry any subset. Additive-only evolution -- existing
//                  members must never be renumbered or removed.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// Bitwise-combinable safety/compliance flags describing the operational
/// hazard profile of an <see cref="IndustryDefinition"/>.
/// </summary>
[Flags]
public enum SafetyFlag
{
    /// <summary>No flags set.</summary>
    None = 0,

    /// <summary>Industrial machinery hazards (rotating/moving equipment).</summary>
    Machinery = 1 << 0,

    /// <summary>Handling, storage, or processing of hazardous chemicals.</summary>
    Chemicals = 1 << 1,

    /// <summary>Presence of flammable materials or atmospheres.</summary>
    Flammables = 1 << 2,

    /// <summary>Shipment of hazardous materials subject to hazmat transport rules.</summary>
    HazmatShipping = 1 << 3,

    /// <summary>Operation of a vehicle fleet.</summary>
    Fleet = 1 << 4,

    /// <summary>Subject to environmental permitting requirements.</summary>
    EnvironmentalPermits = 1 << 5,

    /// <summary>Elevated risk of explosion.</summary>
    ExplosionRisk = 1 << 6,

    /// <summary>Presence of confined spaces requiring entry controls.</summary>
    ConfinedSpaces = 1 << 7,

    /// <summary>Lockout/Tagout energy-control requirements apply.</summary>
    LOTO = 1 << 8,

    /// <summary>Combustible or respirable dust hazards.</summary>
    Dust = 1 << 9,
}
