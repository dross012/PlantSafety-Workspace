// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hazards
//  File:           HazardMetadata.cs
//  Created:        2026-08-08
//  Description:    Phase B.2: a deterministic, static catalog of display
//                  metadata for every HazardType -- mirrors the
//                  IndustryRegistry/EventCatalog precedent elsewhere in
//                  this codebase (a fixed, exhaustive, compile-time list,
//                  not generated/inferred).
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hazards;

/// <summary>Display metadata for a single <see cref="HazardType"/>.</summary>
public sealed record HazardMetadata(HazardType HazardType, HazardCategory Category, string DisplayName, string Description);

/// <summary>The deterministic, exhaustive catalog of <see cref="HazardMetadata"/>, one entry per <see cref="HazardType"/>.</summary>
public static class HazardMetadataCatalog
{
    /// <summary>Every hazard type's metadata, in <see cref="HazardType"/> declaration order.</summary>
    public static readonly IReadOnlyList<HazardMetadata> All = Build();

    /// <summary>Returns the metadata for the given hazard type.</summary>
    public static HazardMetadata ForHazardType(HazardType hazardType) => All.First(entry => entry.HazardType == hazardType);

    private static IReadOnlyList<HazardMetadata> Build() =>
    [
        new(HazardType.Fire, HazardCategoryCatalog.ForHazardType(HazardType.Fire), "Fire", "An uncontrolled fire."),
        new(HazardType.ChemicalSpill, HazardCategoryCatalog.ForHazardType(HazardType.ChemicalSpill), "Chemical Spill", "An uncontrolled release of a hazardous chemical."),
        new(HazardType.Injury, HazardCategoryCatalog.ForHazardType(HazardType.Injury), "Injury", "A physical injury to a person."),
        new(HazardType.EnvironmentalRelease, HazardCategoryCatalog.ForHazardType(HazardType.EnvironmentalRelease), "Environmental Release", "A release into the surrounding environment (air, water, or soil)."),
        new(HazardType.ConfinedSpace, HazardCategoryCatalog.ForHazardType(HazardType.ConfinedSpace), "Confined Space", "A hazard arising from entry into a confined space."),
        new(HazardType.LOTOFailure, HazardCategoryCatalog.ForHazardType(HazardType.LOTOFailure), "Lockout/Tagout Failure", "A failure of lockout/tagout energy-isolation procedures."),
        new(HazardType.HotWork, HazardCategoryCatalog.ForHazardType(HazardType.HotWork), "Hot Work", "A hazard arising from hot-work activity (welding, cutting, or grinding)."),
        new(HazardType.FleetIncident, HazardCategoryCatalog.ForHazardType(HazardType.FleetIncident), "Fleet Incident", "An incident involving a fleet vehicle."),
        new(HazardType.Other, HazardCategoryCatalog.ForHazardType(HazardType.Other), "Other", "A hazard that does not match any other classified type."),
    ];
}
