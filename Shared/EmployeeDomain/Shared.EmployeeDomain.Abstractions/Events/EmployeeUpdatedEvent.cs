// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Events
//  File:           EmployeeUpdatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised whenever an employee's metadata
//                  (name/role) or safety score changes. EmployeeUpdateKind
//                  distinguishes the change reason, mirroring Shared.
//                  FacilityDomain's FacilityUpdateKind precedent.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Events;

/// <summary>The kind of change that produced an <see cref="EmployeeUpdatedEvent"/>.</summary>
public enum EmployeeUpdateKind
{
    /// <summary>The employee's name and/or role changed.</summary>
    Metadata,

    /// <summary>The employee's injury profile changed.</summary>
    InjuryProfile,

    /// <summary>The employee's exposure records changed.</summary>
    ExposureRecords,

    /// <summary>The employee's safety score was recomputed.</summary>
    SafetyScore,
}

/// <summary>Raised when an existing employee's metadata, injury profile, exposure records, or safety score changes.</summary>
public sealed record EmployeeUpdatedEvent(
    EmployeeId EmployeeId,
    EmployeeUpdateKind Kind,
    DateTime OccurredAtUtc);
