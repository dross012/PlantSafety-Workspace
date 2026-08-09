// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Aggregates
//  File:           MachineState.cs
//  Created:        2026-08-08
//  Description:    The immutable data snapshot of a MachineAggregate at a
//                  point in time. MachineAggregate is the only thing
//                  allowed to construct/replace a MachineState -- callers
//                  read it, they don't build it directly.
// ============================================================================
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Failure;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.Maintenance;
using Shared.MachineDomain.Abstractions.Scoring;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Aggregates;

/// <summary>The immutable data snapshot of a <see cref="MachineAggregate"/>.</summary>
public sealed record MachineState(
    MachineId MachineId,
    string Name,
    MachineType MachineType,
    MachineLocation Location,
    MachineMaintenanceProfile MaintenanceProfile,
    IReadOnlyList<MachineFailureRecord> FailureRecords,
    MachineSafetyScore SafetyScore,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
