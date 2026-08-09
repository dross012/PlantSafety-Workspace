// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Failure
//  File:           MachineFailureRecord.cs
//  Created:        2026-08-08
//  Description:    Phase C.2: a single recorded failure event for a
//                  machine. Hazard-aware by construction -- references
//                  Shared.FacilityDomain's unified HazardType vocabulary,
//                  the same pattern used by Shared.EmployeeDomain's
//                  EmployeeExposureRecord, so a failure's hazard type
//                  reads consistently across domains.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

namespace Shared.MachineDomain.Abstractions.Failure;

/// <summary>A single recorded failure event for a machine.</summary>
public sealed record MachineFailureRecord(
    string Description,
    HazardType ResultingHazardType,
    DateTime FailedAtUtc);
