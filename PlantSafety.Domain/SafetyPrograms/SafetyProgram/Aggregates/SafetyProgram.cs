// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgram.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a named safety "program" -- a bundled initiative/campaign
//                  (e.g. "Fall Protection Program", "Hot Work Program") with its own status,
//                  dates, ownership, and participation tracking. Per the SafetyPrograms UI-gaps
//                  scoping decision (docs/Phase5/ui-gaps-scoping.md, section 8), a Program is
//                  deliberately NOT a duplicate of SafetyPolicies/SafetyProcedures/SafetyCulture
//                  content -- it owns only the "program-native" state that has no home in those
//                  three modules (name, description, category, lifecycle status, dates, owner,
//                  participation metrics, and free-text goals). Live rollups of the underlying
//                  policy/procedure/culture-assessment activity for the program's Category are
//                  composed at query time (never snapshotted) by
//                  PlantSafety.Application.SafetyPrograms.Handlers.GetSafetyProgramDetailQueryHandler,
//                  which fans out via ISender to the three sibling modules -- mirroring
//                  PlantSafety.Application.Operations.SystemHealth.Handlers.GetSystemHealthReportQueryHandler's
//                  resilient per-section composition pattern. Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Entities;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Events;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Exceptions;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Aggregates;

/// <summary>Aggregate root for the SafetyProgram bounded context.</summary>
public sealed class SafetyProgram
{
    // ---- Identity ------------------------------------------------------

    /// <summary>Strong-typed identity of this SafetyProgram aggregate.</summary>
    public SafetyProgramId SafetyProgramId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SafetyProgramStatus Status { get; private set; }

    // ---- Business properties --------------------------------------------

    /// <summary>Program name (e.g. "Fall Protection Program").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Narrative description of the program's purpose and scope.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Category this program belongs to, used to select which sibling modules' data
    /// to compose into the live rollup at query time.</summary>
    public SafetyProgramCategory Category { get; private set; }

    /// <summary>Program start date.</summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>Program end date, if scheduled to conclude.</summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>Identity of the accountable program owner.</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>Current participation percentage (0-100), owned/tracked directly by the program
    /// since no leaf module (Policies/Procedures/Culture) models cross-cutting program enrollment.</summary>
    public double ParticipationPercent { get; private set; }

    /// <summary>Count of participants currently enrolled in the program.</summary>
    public int EnrolledCount { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    private readonly List<SafetyProgramGoal> _goals = [];

    /// <summary>Named goals/objectives for this program.</summary>
    public IReadOnlyList<SafetyProgramGoal> Goals => new ReadOnlyCollection<SafetyProgramGoal>(_goals);

    // ---- Domain Events ---------------------------------------------------

    private readonly List<ISafetyProgramDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISafetyProgramDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISafetyProgramDomainEvent>(_domainEvents);

    // For rehydration only.
    private SafetyProgram() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new SafetyProgram aggregate and raises <see cref="SafetyProgramCreatedEvent"/>.</summary>
    public static SafetyProgram Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string name,
        string description,
        SafetyProgramCategory category,
        DateOnly startDate,
        DateOnly? endDate,
        UserId ownerId,
        IReadOnlyList<string>? goals = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Program name must not be empty.", nameof(name));

        var agg = new SafetyProgram
        {
            SafetyProgramId = SafetyProgramId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = SafetyProgramStatus.Planned,
            Name = name.Trim(),
            Description = (description ?? string.Empty).Trim(),
            Category = category,
            StartDate = startDate,
            EndDate = endDate,
            OwnerId = ownerId,
            ParticipationPercent = 0d,
            EnrolledCount = 0,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        if (goals is not null)
        {
            var order = 0;
            foreach (var goalText in goals)
            {
                if (string.IsNullOrWhiteSpace(goalText)) continue;
                agg._goals.Add(SafetyProgramGoal.Create(goalText, order++));
            }
        }

        agg.AddDomainEvent(new SafetyProgramCreatedEvent(
            EventId: Uuid7.New(),
            SafetyProgramId: agg.SafetyProgramId,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle behavior --------------------------------------------------

    /// <summary>Transitions the program to a new lifecycle status, enforcing the canonical
    /// Planned → Active → OnHold ⇄ Active → Completed lifecycle.</summary>
    /// <exception cref="SafetyProgramInvalidStatusTransitionException">Thrown for an illegal transition.</exception>
    public void ChangeStatus(
        SafetyProgramStatus newStatus,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        var allowed = Status switch
        {
            SafetyProgramStatus.Planned => newStatus is SafetyProgramStatus.Active or SafetyProgramStatus.Completed,
            SafetyProgramStatus.Active => newStatus is SafetyProgramStatus.OnHold or SafetyProgramStatus.Completed,
            SafetyProgramStatus.OnHold => newStatus is SafetyProgramStatus.Active or SafetyProgramStatus.Completed,
            SafetyProgramStatus.Completed => false,
            _ => false,
        };

        if (!allowed)
            throw new SafetyProgramInvalidStatusTransitionException(SafetyProgramId, Status, newStatus, correlationId, causationId);

        var previous = Status;
        Status = newStatus;

        AddDomainEvent(new SafetyProgramStatusChangedEvent(
            EventId: Uuid7.New(),
            SafetyProgramId: SafetyProgramId,
            PreviousStatus: previous,
            NewStatus: newStatus,
            ChangedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Updates the program's own participation/enrollment tracking metrics.</summary>
    public void RecordParticipation(double participationPercent, int enrolledCount)
    {
        if (participationPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(participationPercent), "ParticipationPercent must be within [0, 100].");
        if (enrolledCount < 0)
            throw new ArgumentOutOfRangeException(nameof(enrolledCount), "EnrolledCount must not be negative.");

        ParticipationPercent = participationPercent;
        EnrolledCount = enrolledCount;
    }

    /// <summary>Updates the program's descriptive/scheduling details. Status is managed separately via <see cref="ChangeStatus"/>.</summary>
    public void UpdateDetails(string name, string description, DateOnly startDate, DateOnly? endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Program name must not be empty.", nameof(name));

        Name = name.Trim();
        Description = (description ?? string.Empty).Trim();
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>Replaces the program's goal list.</summary>
    public void ReplaceGoals(IReadOnlyList<string> goals)
    {
        _goals.Clear();
        var order = 0;
        foreach (var goalText in goals)
        {
            if (string.IsNullOrWhiteSpace(goalText)) continue;
            _goals.Add(SafetyProgramGoal.Create(goalText, order++));
        }
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISafetyProgramDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
