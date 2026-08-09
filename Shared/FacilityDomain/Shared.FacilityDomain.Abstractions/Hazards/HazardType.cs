// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hazards
//  File:           HazardType.cs
//  Created:        2026-08-08
//  Description:    Phase B.2: the unified hazard-type vocabulary shared
//                  across every domain (AccidentManagement, SafetyEvents,
//                  CrisisManagement, EmergencyManagement) once Phase B.3
//                  wires their enriched V2 events to it. Additive-only: new
//                  members may be appended, existing members must never be
//                  renumbered or removed once published.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hazards;

/// <summary>The unified hazard-type vocabulary.</summary>
public enum HazardType
{
    /// <summary>An uncontrolled fire.</summary>
    Fire,

    /// <summary>An uncontrolled release of a hazardous chemical.</summary>
    ChemicalSpill,

    /// <summary>A physical injury to a person.</summary>
    Injury,

    /// <summary>A release into the surrounding environment (air, water, soil).</summary>
    EnvironmentalRelease,

    /// <summary>A hazard arising from entry into a confined space.</summary>
    ConfinedSpace,

    /// <summary>A failure of lockout/tagout energy-isolation procedures.</summary>
    LOTOFailure,

    /// <summary>A hazard arising from hot-work activity (welding, cutting, grinding).</summary>
    HotWork,

    /// <summary>An incident involving a fleet vehicle.</summary>
    FleetIncident,

    /// <summary>A hazard that does not match any of the above -- the classification default, never a data-entry error.</summary>
    Other,
}
