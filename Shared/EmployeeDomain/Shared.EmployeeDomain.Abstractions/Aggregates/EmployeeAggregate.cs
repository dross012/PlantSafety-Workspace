// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Aggregates
//  File:           EmployeeAggregate.cs
//  Created:        2026-08-08
//  Description:    The Employee aggregate root. Owns an EmployeeState
//                  snapshot, replaced immutably (via `with`) by every
//                  command, and collects the domain event each command
//                  raises so a caller (EmployeeService, in the
//                  Infrastructure layer) can persist the new state. No
//                  reflection, no dynamic typing -- every command is a
//                  plain, fully typed method. Mirrors Shared.FacilityDomain's
//                  FacilityAggregate pattern.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Events;
using Shared.EmployeeDomain.Abstractions.Exposure;
using Shared.EmployeeDomain.Abstractions.Injury;
using Shared.EmployeeDomain.Abstractions.Scoring;
using Shared.EmployeeDomain.Abstractions.Training;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Aggregates;

/// <summary>The Employee aggregate root.</summary>
public sealed class EmployeeAggregate
{
    private readonly List<object> _domainEvents = [];

    private EmployeeAggregate(EmployeeState state) => State = state;

    /// <summary>The aggregate's current, immutable state snapshot.</summary>
    public EmployeeState State { get; private set; }

    /// <summary>Domain events raised since construction/rehydration, in the order they occurred.</summary>
    public IReadOnlyList<object> DomainEvents => _domainEvents;

    /// <summary>Creates a new employee. Training/certifications/exposure/injury history start empty.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public static EmployeeAggregate CreateEmployee(EmployeeId employeeId, string name, EmployeeRole role, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Employee name is required.", nameof(name));
        }

        var state = new EmployeeState(
            EmployeeId: employeeId,
            Name: name,
            Role: role,
            TrainingProfile: new EmployeeTrainingProfile([], nowUtc),
            Certifications: [],
            ExposureRecords: [],
            InjuryProfile: EmployeeInjuryProfile.Empty(nowUtc),
            SafetyScore: EmployeeSafetyScore.Compute(injuryCount: 0, exposureCount: 0, certificationCount: 0, completedTrainingCount: 0, nowUtc),
            CreatedAtUtc: nowUtc,
            UpdatedAtUtc: nowUtc);

        var aggregate = new EmployeeAggregate(state);
        aggregate._domainEvents.Add(new EmployeeCreatedEvent(employeeId, name, role, nowUtc));
        return aggregate;
    }

    /// <summary>Rehydrates an aggregate from a previously persisted state, with no domain events pending.</summary>
    public static EmployeeAggregate Rehydrate(EmployeeState state) => new(state);

    /// <summary>Updates the employee's name/role.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public void UpdateEmployeeMetadata(string name, EmployeeRole role, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Employee name is required.", nameof(name));
        }

        State = State with { Name = name, Role = role, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new EmployeeUpdatedEvent(State.EmployeeId, EmployeeUpdateKind.Metadata, nowUtc));
    }

    /// <summary>Replaces the employee's completed training codes and recomputes the safety score.</summary>
    public void UpdateTrainingProfile(IReadOnlyList<string> completedTrainingCodes, DateTime nowUtc)
    {
        State = State with { TrainingProfile = new EmployeeTrainingProfile(completedTrainingCodes, nowUtc), UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new EmployeeTrainingUpdatedEvent(State.EmployeeId, completedTrainingCodes.Count, nowUtc));
        RecomputeSafetyScore(nowUtc);
    }

    /// <summary>Replaces the employee's certifications and recomputes the safety score.</summary>
    public void UpdateCertifications(IReadOnlyList<EmployeeCertification> certifications, DateTime nowUtc)
    {
        State = State with { Certifications = certifications, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new EmployeeCertificationUpdatedEvent(State.EmployeeId, certifications.Count, nowUtc));
        RecomputeSafetyScore(nowUtc);
    }

    /// <summary>Appends a hazard exposure record and recomputes the safety score.</summary>
    public void RecordExposure(EmployeeExposureRecord exposureRecord, DateTime nowUtc)
    {
        State = State with { ExposureRecords = [.. State.ExposureRecords, exposureRecord], UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new EmployeeUpdatedEvent(State.EmployeeId, EmployeeUpdateKind.ExposureRecords, nowUtc));
        RecomputeSafetyScore(nowUtc);
    }

    /// <summary>Records a new injury and recomputes the safety score.</summary>
    /// <exception cref="ArgumentException"><paramref name="description"/> is null or whitespace.</exception>
    public void RecordInjury(string description, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Injury description is required.", nameof(description));
        }

        State = State with { InjuryProfile = State.InjuryProfile.RecordInjury(description, nowUtc), UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new EmployeeUpdatedEvent(State.EmployeeId, EmployeeUpdateKind.InjuryProfile, nowUtc));
        RecomputeSafetyScore(nowUtc);
    }

    /// <summary>Clears every pending domain event, e.g. after a caller has persisted/published them.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void RecomputeSafetyScore(DateTime nowUtc)
    {
        EmployeeSafetyScore score = EmployeeSafetyScore.Compute(
            injuryCount: State.InjuryProfile.TotalInjuryCount,
            exposureCount: State.ExposureRecords.Count,
            certificationCount: State.Certifications.Count,
            completedTrainingCount: State.TrainingProfile.CompletedTrainingCodes.Count,
            nowUtc);

        State = State with { SafetyScore = score, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new EmployeeUpdatedEvent(State.EmployeeId, EmployeeUpdateKind.SafetyScore, nowUtc));
    }
}
