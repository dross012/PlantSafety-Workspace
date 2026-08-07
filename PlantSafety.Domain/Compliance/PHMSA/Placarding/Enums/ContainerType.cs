// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           ContainerType.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Physical container/packaging form factor for a hazmat load, used to determine
//                  which marking/labeling/placarding rules apply.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;

/// <summary>Physical container/packaging form factor tracked by the HazmatContainer aggregate.</summary>
public enum ContainerType
{
    /// <summary>Steel, plastic, or fiber drum.</summary>
    Drum = 0,

    /// <summary>Intermediate Bulk Container (IBC/tote).</summary>
    IntermediateBulkContainer = 1,

    /// <summary>Compressed/liquefied gas cylinder.</summary>
    Cylinder = 2,

    /// <summary>Rigid or flexible tote.</summary>
    Tote = 3,

    /// <summary>Overpack enclosing one or more inner packagings.</summary>
    Overpack = 4,

    /// <summary>Salvage drum used to contain a damaged/leaking package.</summary>
    SalvageDrum = 5,

    /// <summary>Bulk tank (cargo tank, portable tank, tank car).</summary>
    BulkTank = 6,

    /// <summary>Portable tank.</summary>
    Portable = 7,
}
