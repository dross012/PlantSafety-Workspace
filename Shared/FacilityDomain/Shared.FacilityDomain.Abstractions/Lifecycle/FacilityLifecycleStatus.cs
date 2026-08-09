// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Lifecycle
//  File:           FacilityLifecycleStatus.cs
//  Created:        2026-08-08
//  Description:    Phase C.12 (Path C: Operational Modernization): a
//                  facility's operational lifecycle state. A newly created
//                  facility starts PendingActivation; ActivateFacility,
//                  ShutdownFacility, ReopenFacility, and DecommissionFacility
//                  on FacilityAggregate are the only valid transitions
//                  between these states (see that file's own guard clauses).
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Lifecycle;

/// <summary>A facility's operational lifecycle state.</summary>
public enum FacilityLifecycleStatus
{
    /// <summary>Created but not yet activated for operation.</summary>
    PendingActivation,

    /// <summary>Actively operating.</summary>
    Active,

    /// <summary>Temporarily shut down; may be reopened.</summary>
    ShutDown,

    /// <summary>Permanently decommissioned. Terminal -- no further lifecycle transitions are valid.</summary>
    Decommissioned,
}
