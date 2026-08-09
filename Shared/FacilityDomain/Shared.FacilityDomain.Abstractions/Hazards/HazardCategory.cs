// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hazards
//  File:           HazardCategory.cs
//  Created:        2026-08-08
//  Description:    Phase B.2: a coarser grouping over HazardType, for
//                  dashboard/report rollups where the full 9-way
//                  granularity is too fine. HazardCategoryCatalog's mapping
//                  is a documented editorial judgment (mirrors the existing
//                  precedent set by Shared.SafetyData's
//                  RegulatorCategoryRiskCatalog -- a per-category judgment
//                  with no mechanical derivation available), not a
//                  mechanically derived fact.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hazards;

/// <summary>A coarse grouping over <see cref="HazardType"/>.</summary>
public enum HazardCategory
{
    /// <summary>Hazards whose primary consequence is direct physical harm to a person.</summary>
    PhysicalSafety,

    /// <summary>Hazards involving a hazardous substance or its release into the environment.</summary>
    ChemicalEnvironmental,

    /// <summary>Hazards arising from a failure in an operational/process-safety procedure.</summary>
    OperationalProcess,

    /// <summary>Hazards involving a vehicle or fleet asset.</summary>
    Transportation,

    /// <summary>No category applies -- reserved for <see cref="HazardType.Other"/>.</summary>
    Uncategorized,
}

/// <summary>Maps every <see cref="HazardType"/> to its <see cref="HazardCategory"/>.</summary>
public static class HazardCategoryCatalog
{
    /// <summary>Returns the category for the given hazard type.</summary>
    public static HazardCategory ForHazardType(HazardType hazardType) => hazardType switch
    {
        HazardType.Fire => HazardCategory.PhysicalSafety,
        HazardType.Injury => HazardCategory.PhysicalSafety,
        HazardType.ConfinedSpace => HazardCategory.PhysicalSafety,
        HazardType.HotWork => HazardCategory.PhysicalSafety,
        HazardType.LOTOFailure => HazardCategory.OperationalProcess,
        HazardType.ChemicalSpill => HazardCategory.ChemicalEnvironmental,
        HazardType.EnvironmentalRelease => HazardCategory.ChemicalEnvironmental,
        HazardType.FleetIncident => HazardCategory.Transportation,
        HazardType.Other => HazardCategory.Uncategorized,
        _ => throw new ArgumentOutOfRangeException(nameof(hazardType), hazardType, "Unknown hazard type."),
    };
}
