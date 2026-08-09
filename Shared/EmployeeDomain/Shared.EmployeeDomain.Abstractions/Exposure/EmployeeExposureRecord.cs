// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Exposure
//  File:           EmployeeExposureRecord.cs
//  Created:        2026-08-08
//  Description:    Phase C.1: a single recorded hazard exposure event for
//                  an employee. Hazard-aware by construction -- references
//                  Shared.FacilityDomain's unified HazardType vocabulary
//                  rather than inventing a parallel one, per Path C's
//                  "hazard propagation modeling" requirement.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

namespace Shared.EmployeeDomain.Abstractions.Exposure;

/// <summary>A single recorded hazard exposure event for an employee.</summary>
public sealed record EmployeeExposureRecord(
    HazardType HazardType,
    string Description,
    DateTime ExposedAtUtc);
