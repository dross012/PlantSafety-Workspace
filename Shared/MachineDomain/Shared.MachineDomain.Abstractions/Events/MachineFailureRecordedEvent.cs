// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Events
//  File:           MachineFailureRecordedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by MachineAggregate.RecordFailure.
//                  Carries the resulting HazardType so downstream hazard
//                  propagation (Phase C.6) can react without re-deriving it.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Events;

/// <summary>Raised when a failure is recorded against a machine.</summary>
public sealed record MachineFailureRecordedEvent(
    MachineId MachineId,
    string Description,
    HazardType ResultingHazardType,
    DateTime OccurredAtUtc);
