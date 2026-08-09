// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Triggers
//  File:           EvacuationTrigger.cs
//  Created:        2026-08-08
//  Description:    Phase C.7: a personnel evacuation triggered by a
//                  workflow evaluation. EvacuationScope maps onto
//                  Shared.FacilityDomain's Phase C.5 hierarchy levels
//                  (Zone/Area/Building) plus a whole-Facility scope.
// ============================================================================
namespace Shared.WorkflowEngine.Abstractions.Triggers;

/// <summary>How broad an evacuation is, mapping onto Shared.FacilityDomain's Building/Area/Zone hierarchy.</summary>
public enum EvacuationScope
{
    /// <summary>Evacuate a single zone.</summary>
    Zone,

    /// <summary>Evacuate a single area.</summary>
    Area,

    /// <summary>Evacuate a single building.</summary>
    Building,

    /// <summary>Evacuate the entire facility.</summary>
    Facility,
}

/// <summary>A personnel evacuation triggered by a workflow evaluation.</summary>
public sealed record EvacuationTrigger(
    EvacuationScope Scope,
    string Reason);
