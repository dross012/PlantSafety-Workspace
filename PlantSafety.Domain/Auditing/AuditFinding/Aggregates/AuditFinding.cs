// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFinding.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a single non-conformance/finding raised during an AuditEngagement, together with its corrective-action assignment, due date, and verification state.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Aggregates;

/// <summary>Aggregate root for the AuditFinding bounded context.</summary>
public sealed partial class AuditFinding
{
    // ---- Identity ------------------------------------------------------

    /// <summary>Strong-typed identity of this AuditFinding aggregate.</summary>
    public AuditFindingId AuditFindingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AuditFindingStatus Status { get; private set; }

    // ---- Business properties --------------------------------------------
    /// <summary>Identity of the parent AuditEngagement this finding was raised against.</summary>
    public Guid AuditEngagementId { get; private set; }

    /// <summary>Short finding title.</summary>
    public string FindingTitle { get; private set; } = string.Empty;

    /// <summary>Severity classification: Low, Medium, High, Critical.</summary>
    public string Severity { get; private set; } = "Medium";

    /// <summary>Whether a corrective action is required to close this finding.</summary>
    public bool CorrectiveActionRequired { get; private set; }

    /// <summary>Identity of the user assigned to remediate this finding, if assigned.</summary>
    public UserId? AssignedToUserId { get; private set; }

    /// <summary>Corrective action due date, if assigned.</summary>
    public UtcTimestamp? DueDateUtc { get; private set; }

    /// <summary>UTC timestamp the corrective action was verified as effective, if verified.</summary>
    public UtcTimestamp? VerifiedAtUtc { get; private set; }

    /// <summary>Narrative of the corrective action actually performed, once remediated.</summary>
    public string? RemediationNotes { get; private set; }

    /// <summary>UTC timestamp the corrective action was performed (remediated), if remediated.</summary>
    public UtcTimestamp? RemediatedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------

    private readonly List<IAuditFindingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAuditFindingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAuditFindingDomainEvent>(_domainEvents);

    // For rehydration only.
    private AuditFinding() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new AuditFinding aggregate and raises <see cref="AuditFindingCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    public static AuditFinding Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid auditEngagementId = default,
        string findingTitle = "",
        string severity = "Medium",
        bool correctiveActionRequired = false,
        UserId? assignedToUserId = null,
        UtcTimestamp? dueDateUtc = null)
    {
        var agg = new AuditFinding
        {
            AuditFindingId      = AuditFindingId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = AuditFindingStatus.Created,
            AuditEngagementId         = auditEngagementId,
            FindingTitle              = (findingTitle ?? string.Empty).Trim(),
            Severity                  = (severity ?? "Medium").Trim(),
            CorrectiveActionRequired  = correctiveActionRequired,
            AssignedToUserId          = assignedToUserId,
            DueDateUtc                = dueDateUtc,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new AuditFindingCreatedEvent(
            EventId:       new AuditFindingEventId(Uuid7.New()),
            AuditFindingId:      agg.AuditFindingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle behavior --------------------------------------------------

    /// <summary>
    /// Triggers a Corrective and Preventive Action (CAPA) for this finding: assigns a
    /// responsible user and due date. Per PlantSafety Program Structure "Auditing Module" and
    /// Appendix G (Corrective Action Categories), <c>High</c> and <c>Critical</c> severity
    /// findings always require a CAPA; <c>Low</c>/<c>Medium</c> findings may trigger one at the
    /// auditor's discretion.
    /// </summary>
    /// <exception cref="AuditFindingInvalidStatusTransitionException">Thrown when the finding has already been closed or archived.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dueDateUtc"/> is empty.</exception>
    public void TriggerCapa(
        UserId assignedToUserId,
        UtcTimestamp dueDateUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        EnsureStatusIsOneOf(AuditFindingStatus.Created, AuditFindingStatus.Active);

        if (dueDateUtc.IsEmpty)
            throw new ArgumentException("DueDateUtc must be provided when triggering a CAPA.", nameof(dueDateUtc));

        AssignedToUserId = assignedToUserId;
        DueDateUtc = dueDateUtc;
        CorrectiveActionRequired = true;
        Status = AuditFindingStatus.Active;

        AddDomainEvent(new AuditFindingCapaTriggeredEvent(
            EventId: new AuditFindingEventId(Uuid7.New()),
            AuditFindingId: AuditFindingId,
            AssignedToUserId: assignedToUserId,
            DueDateUtc: dueDateUtc,
            Severity: Severity,
            TriggeredAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records that the assigned corrective action has been performed (Active → Completed),
    /// awaiting effectiveness verification.
    /// </summary>
    /// <exception cref="AuditFindingInvalidStatusTransitionException">Thrown when no CAPA is pending.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="remediationNotes"/> is empty.</exception>
    public void Remediate(
        string remediationNotes,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != AuditFindingStatus.Active)
            throw new AuditFindingInvalidStatusTransitionException(
                AuditFindingId, Status.ToString(), AuditFindingStatus.Completed.ToString(), correlationId, causationId);

        if (string.IsNullOrWhiteSpace(remediationNotes))
            throw new ArgumentException("RemediationNotes must not be empty.", nameof(remediationNotes));

        RemediationNotes = remediationNotes.Trim();
        RemediatedAtUtc = occurredAtUtc;
        Status = AuditFindingStatus.Completed;

        AddDomainEvent(new AuditFindingRemediatedEvent(
            EventId: new AuditFindingEventId(Uuid7.New()),
            AuditFindingId: AuditFindingId,
            RemediationNotes: RemediationNotes,
            RemediatedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Verifies the effectiveness of the completed corrective action and closes this finding
    /// (Completed → Closed).
    /// </summary>
    /// <exception cref="AuditFindingInvalidStatusTransitionException">Thrown when remediation has not yet been recorded.</exception>
    public void Verify(
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != AuditFindingStatus.Completed)
            throw new AuditFindingInvalidStatusTransitionException(
                AuditFindingId, Status.ToString(), AuditFindingStatus.Closed.ToString(), correlationId, causationId);

        VerifiedAtUtc = occurredAtUtc;
        Status = AuditFindingStatus.Closed;

        AddDomainEvent(new AuditFindingVerifiedEvent(
            EventId: new AuditFindingEventId(Uuid7.New()),
            AuditFindingId: AuditFindingId,
            VerifiedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAuditFindingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AuditFindingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            "Invalid lifecycle transition from '" + Status + "' for AuditFinding " + AuditFindingId + ".");
    }
}