// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Aggregates
//  File:           MachineAggregate.cs
//  Created:        2026-08-08
//  Description:    The Machine aggregate root. Owns a MachineState
//                  snapshot, replaced immutably (via `with`) by every
//                  command, and collects the domain event each command
//                  raises so a caller (MachineService, in the
//                  Infrastructure layer) can persist the new state. No
//                  reflection, no dynamic typing -- every command is a
//                  plain, fully typed method. Mirrors Shared.FacilityDomain's
//                  FacilityAggregate / Shared.EmployeeDomain's
//                  EmployeeAggregate pattern.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Events;
using Shared.MachineDomain.Abstractions.Failure;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.Maintenance;
using Shared.MachineDomain.Abstractions.Scoring;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Aggregates;

/// <summary>The Machine aggregate root.</summary>
public sealed class MachineAggregate
{
    private readonly List<object> _domainEvents = [];

    private MachineAggregate(MachineState state) => State = state;

    /// <summary>The aggregate's current, immutable state snapshot.</summary>
    public MachineState State { get; private set; }

    /// <summary>Domain events raised since construction/rehydration, in the order they occurred.</summary>
    public IReadOnlyList<object> DomainEvents => _domainEvents;

    /// <summary>Creates a new machine. Its maintenance profile starts unscheduled and it has no failure history.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public static MachineAggregate CreateMachine(MachineId machineId, string name, MachineType machineType, MachineLocation location, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Machine name is required.", nameof(name));
        }

        var state = new MachineState(
            MachineId: machineId,
            Name: name,
            MachineType: machineType,
            Location: location,
            MaintenanceProfile: new MachineMaintenanceProfile(null, null, MaintenanceIntervalDays: 0, nowUtc),
            FailureRecords: [],
            SafetyScore: MachineSafetyScore.Compute(failureCount: 0, maintenanceOverdue: false, nowUtc),
            CreatedAtUtc: nowUtc,
            UpdatedAtUtc: nowUtc);

        var aggregate = new MachineAggregate(state);
        aggregate._domainEvents.Add(new MachineCreatedEvent(machineId, name, machineType, location, nowUtc));
        return aggregate;
    }

    /// <summary>Rehydrates an aggregate from a previously persisted state, with no domain events pending.</summary>
    public static MachineAggregate Rehydrate(MachineState state) => new(state);

    /// <summary>Updates the machine's name/type/location.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public void UpdateMachineMetadata(string name, MachineType machineType, MachineLocation location, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Machine name is required.", nameof(name));
        }

        State = State with { Name = name, MachineType = machineType, Location = location, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new MachineUpdatedEvent(State.MachineId, MachineUpdateKind.Metadata, nowUtc));
    }

    /// <summary>Replaces the machine's maintenance profile and recomputes the safety score.</summary>
    public void UpdateMaintenanceProfile(DateTime? lastMaintenanceAtUtc, DateTime? nextScheduledMaintenanceAtUtc, int maintenanceIntervalDays, DateTime nowUtc)
    {
        State = State with { MaintenanceProfile = new MachineMaintenanceProfile(lastMaintenanceAtUtc, nextScheduledMaintenanceAtUtc, maintenanceIntervalDays, nowUtc), UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new MachineMaintenanceUpdatedEvent(State.MachineId, nextScheduledMaintenanceAtUtc, nowUtc));
        RecomputeSafetyScore(nowUtc);
    }

    /// <summary>Records a new failure and recomputes the safety score.</summary>
    /// <exception cref="ArgumentException"><paramref name="description"/> is null or whitespace.</exception>
    public void RecordFailure(string description, HazardType resultingHazardType, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Failure description is required.", nameof(description));
        }

        var record = new MachineFailureRecord(description, resultingHazardType, nowUtc);
        State = State with { FailureRecords = [.. State.FailureRecords, record], UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new MachineFailureRecordedEvent(State.MachineId, description, resultingHazardType, nowUtc));
        RecomputeSafetyScore(nowUtc);
    }

    /// <summary>Clears every pending domain event, e.g. after a caller has persisted/published them.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void RecomputeSafetyScore(DateTime nowUtc)
    {
        MachineSafetyScore score = MachineSafetyScore.Compute(
            failureCount: State.FailureRecords.Count,
            maintenanceOverdue: State.MaintenanceProfile.IsOverdue(nowUtc),
            nowUtc);

        State = State with { SafetyScore = score, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new MachineUpdatedEvent(State.MachineId, MachineUpdateKind.SafetyScore, nowUtc));
    }
}
