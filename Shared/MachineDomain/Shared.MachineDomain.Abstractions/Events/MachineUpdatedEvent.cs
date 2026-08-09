// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Events
//  File:           MachineUpdatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised whenever a machine's metadata
//                  (name/type/location) or safety score changes.
//                  MachineUpdateKind distinguishes the change reason,
//                  mirroring Shared.FacilityDomain's FacilityUpdateKind
//                  and Shared.EmployeeDomain's EmployeeUpdateKind precedent.
// ============================================================================
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Events;

/// <summary>The kind of change that produced a <see cref="MachineUpdatedEvent"/>.</summary>
public enum MachineUpdateKind
{
    /// <summary>The machine's name/type/location changed.</summary>
    Metadata,

    /// <summary>The machine's safety score was recomputed.</summary>
    SafetyScore,
}

/// <summary>Raised when an existing machine's metadata or safety score changes.</summary>
public sealed record MachineUpdatedEvent(
    MachineId MachineId,
    MachineUpdateKind Kind,
    DateTime OccurredAtUtc);
