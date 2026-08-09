// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Events
//  File:           MachineMaintenanceUpdatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by MachineAggregate.UpdateMaintenanceProfile.
// ============================================================================
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Events;

/// <summary>Raised when a machine's maintenance profile is replaced.</summary>
public sealed record MachineMaintenanceUpdatedEvent(
    MachineId MachineId,
    DateTime? NextScheduledMaintenanceAtUtc,
    DateTime OccurredAtUtc);
