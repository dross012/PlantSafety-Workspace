// ============================================================================
//  Project:        AI.HazardUnderstanding
//  Module:         Abstractions.Ontology
//  File:           HazardOntology.cs
//  Created:        2026-08-08
//  Description:    Phase E.2 (Path E: AI-Driven Safety Automation): a
//                  deterministic semantic network of related hazard types
//                  -- which hazards commonly co-occur with or lead into
//                  one another. Categorization already exists (Shared.
//                  FacilityDomain's Phase B.2 HazardCategory/
//                  HazardCategoryCatalog); this file's job is the
//                  relationship graph a category rollup can't express, so
//                  it deliberately does not redefine a HazardCategory of
//                  its own -- HazardUnderstandingEngine reuses the real one.
//                  A fixed lookup table, documented editorial judgment
//                  (mirrors HazardCategoryCatalog's own precedent), not a
//                  mechanically derived fact.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

namespace AI.HazardUnderstanding.Abstractions.Ontology;

/// <summary>A deterministic semantic network of hazard types commonly related to one another.</summary>
public static class HazardOntology
{
    private static readonly IReadOnlyDictionary<HazardType, IReadOnlyList<HazardType>> RelatedHazardsByType = new Dictionary<HazardType, IReadOnlyList<HazardType>>
    {
        [HazardType.Fire] = [HazardType.ChemicalSpill, HazardType.HotWork, HazardType.Injury],
        [HazardType.ChemicalSpill] = [HazardType.Fire, HazardType.EnvironmentalRelease, HazardType.Injury],
        [HazardType.EnvironmentalRelease] = [HazardType.ChemicalSpill],
        [HazardType.Injury] = [HazardType.Fire, HazardType.ChemicalSpill, HazardType.LOTOFailure, HazardType.ConfinedSpace],
        [HazardType.ConfinedSpace] = [HazardType.Injury, HazardType.EnvironmentalRelease],
        [HazardType.LOTOFailure] = [HazardType.Injury, HazardType.HotWork],
        [HazardType.HotWork] = [HazardType.Fire, HazardType.LOTOFailure],
        [HazardType.FleetIncident] = [HazardType.Injury],
        [HazardType.Other] = [],
    };

    /// <summary>Returns every hazard type commonly related to (co-occurring with or leading into) the given hazard type.</summary>
    public static IReadOnlyList<HazardType> RelatedHazardsOf(HazardType hazardType) =>
        RelatedHazardsByType.TryGetValue(hazardType, out IReadOnlyList<HazardType>? related) ? related : [];
}
