// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Engine
//  File:           WorkflowContext.cs
//  Created:        2026-08-08
//  Description:    Phase C.7: the deterministic inputs WorkflowEngine.Evaluate
//                  reacts to -- severity, hazard type, employee exposure,
//                  machine failure, and multi-hazard chain length, per
//                  Path C's spec. EvacuationScope is supplied by the caller
//                  (derived from where in the FacilityHierarchy the event
//                  occurred) rather than re-derived here, since only the
//                  caller knows which Building/Area/Zone is affected.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Triggers;

namespace Shared.WorkflowEngine.Abstractions.Engine;

/// <summary>The deterministic inputs a workflow evaluation reacts to.</summary>
public sealed record WorkflowContext(
    HazardType HazardType,
    SeverityLevel SeverityLevel,
    EvacuationScope AffectedScope,
    int EmployeeExposureCount,
    int MachineFailureCount,
    int EventChainLength);
