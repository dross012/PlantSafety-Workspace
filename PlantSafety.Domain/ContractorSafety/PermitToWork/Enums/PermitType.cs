// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of Permit-to-Work (PTW) types per the PlantSafety Program Structure
//                  PTW component: hot work, confined space entry, electrical/isolation, working at
//                  height, and excavation permits.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Enums;

/// <summary>Classification of Permit-to-Work (PTW) types.</summary>
public enum PermitType
{
    /// <summary>Hot work permit (welding, cutting, grinding).</summary>
    HotWork = 0,

    /// <summary>Confined space entry permit (tanks, pits, silos).</summary>
    ConfinedSpaceEntry = 1,

    /// <summary>Electrical/isolation permit (LOTO procedures, energized work).</summary>
    ElectricalIsolation = 2,

    /// <summary>Working at height permit (scaffolding, elevated platforms).</summary>
    WorkingAtHeight = 3,

    /// <summary>Excavation permit (trenching, underground utilities).</summary>
    Excavation = 4,
}
