// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignment.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PPEAssignment bounded context (module: PPE).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Aggregates;

/// <summary>Aggregate root for the PPEAssignment bounded context.</summary>
public sealed partial class PPEAssignment
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPEAssignment aggregate.</summary>
    public PPEAssignmentId PPEAssignmentId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPEAssignmentStatus Status { get; private set; }

    // ---- PPEAssignment-specific business properties --------------------------------

    /// <summary>WorkerId business attribute.</summary>
    public Guid WorkerId { get; private set; }

    /// <summary>JobRole business attribute.</summary>
    public string JobRole { get; private set; }

    /// <summary>HazardZone business attribute.</summary>
    public string HazardZone { get; private set; }

    /// <summary>AssignedPpeItems business attribute.</summary>
    public string AssignedPpeItems { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPEAssignmentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPEAssignmentDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPEAssignmentDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPEAssignment() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPEAssignment aggregate and raises <see cref="PPEAssignmentCreatedEvent"/>.</summary>
    public static PPEAssignment Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid workerId = default,
        string jobRole = "",
        string hazardZone = "",
        string assignedPpeItems = "")
    {
        var agg = new PPEAssignment
        {
            PPEAssignmentId     = PPEAssignmentId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PPEAssignmentStatus.Draft,
            WorkerId = workerId,
            JobRole = (jobRole ?? string.Empty).Trim(),
            HazardZone = (hazardZone ?? string.Empty).Trim(),
            AssignedPpeItems = (assignedPpeItems ?? string.Empty).Trim(),
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PPEAssignmentCreatedEvent(
            EventId:       new PPEAssignmentEventId(Uuid7.New()),
            PPEAssignmentId:      agg.PPEAssignmentId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>UTC instant the fit test result expires and re-testing is required.</summary>
    public UtcTimestamp? FitTestExpiresAtUtc { get; private set; }

    /// <summary>Reason recorded when the assignment is flagged for replacement.</summary>
    public string? ReplacementReason { get; private set; }

    // ---- Lifecycle: PPE Assignment & Compliance ------------------------------

    /// <summary>Issues the assigned PPE to the worker ("PPE assignment profiles"). Valid only from <see cref="PPEAssignmentStatus.Draft"/>.</summary>
    public void Assign(UtcTimestamp issuedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEAssignmentStatus.Draft);

        Status = PPEAssignmentStatus.Assigned;

        AddDomainEvent(new PPEAssignmentIssuedEvent(
            EventId: new PPEAssignmentEventId(Uuid7.New()),
            PPEAssignmentId: PPEAssignmentId,
            IssuedAtUtc: issuedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records a respirator/PPE fit test result ("PPE Training & Fit Testing"). Passing results
    /// verify the assignment; failing results flag the assignment for replacement.
    /// Valid from <see cref="PPEAssignmentStatus.Assigned"/> or <see cref="PPEAssignmentStatus.FitTestVerified"/> (re-test).
    /// </summary>
    public void RecordFitTest(bool passed, string? notes, UtcTimestamp testedAtUtc, UtcTimestamp? fitTestExpiresAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEAssignmentStatus.Assigned, PPEAssignmentStatus.FitTestVerified);

        FitTestExpiresAtUtc = fitTestExpiresAtUtc;
        Status = passed ? PPEAssignmentStatus.FitTestVerified : PPEAssignmentStatus.PendingReplacement;
        if (!passed)
            ReplacementReason = string.IsNullOrWhiteSpace(notes) ? "Failed fit test." : notes.Trim();

        AddDomainEvent(new PPEAssignmentFitTestRecordedEvent(
            EventId: new PPEAssignmentEventId(Uuid7.New()),
            PPEAssignmentId: PPEAssignmentId,
            Passed: passed,
            Notes: notes,
            TestedAtUtc: testedAtUtc,
            FitTestExpiresAtUtc: fitTestExpiresAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Flags the assignment for PPE replacement (damage, expiration, non-compliance observed via Observability). Valid from <see cref="PPEAssignmentStatus.Assigned"/> or <see cref="PPEAssignmentStatus.FitTestVerified"/>.</summary>
    public void FlagForReplacement(string reason, UtcTimestamp flaggedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEAssignmentStatus.Assigned, PPEAssignmentStatus.FitTestVerified);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A replacement reason must be supplied.", nameof(reason));

        ReplacementReason = reason.Trim();
        Status = PPEAssignmentStatus.PendingReplacement;

        AddDomainEvent(new PPEAssignmentFlaggedForReplacementEvent(
            EventId: new PPEAssignmentEventId(Uuid7.New()),
            PPEAssignmentId: PPEAssignmentId,
            Reason: ReplacementReason,
            FlaggedAtUtc: flaggedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Retires the assignment (worker reassigned, terminated, or PPE permanently returned).</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PPEAssignmentStatus.Assigned, PPEAssignmentStatus.FitTestVerified, PPEAssignmentStatus.PendingReplacement);

        Status = PPEAssignmentStatus.Retired;

        AddDomainEvent(new PPEAssignmentRetiredEvent(
            EventId: new PPEAssignmentEventId(Uuid7.New()),
            PPEAssignmentId: PPEAssignmentId,
            RetiredAtUtc: retiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPEAssignmentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPEAssignmentStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPEAssignment {PPEAssignmentId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params PPEAssignmentStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new PPEAssignmentInvalidStatusTransitionException(
            PPEAssignmentId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}