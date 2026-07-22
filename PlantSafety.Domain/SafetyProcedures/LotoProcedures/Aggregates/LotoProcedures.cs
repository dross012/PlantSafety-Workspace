// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProcedures.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single LOTO procedure document (distinct from
//                  EmployeeSafety.LotoPrograms and MachineSafety.LotoSystems, which model individual
//                  isolation instances/equipment). This aggregate models the governing written
//                  procedure: energy source identification, isolation point documentation, authorized
//                  personnel roster, and the mandatory annual audit lifecycle.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates;

/// <summary>Aggregate root for a single LOTO governing procedure document.</summary>
public sealed partial class LotoProcedures
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this LotoProcedures aggregate.</summary>
    public LotoProceduresId LotoProceduresId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public LotoProceduresStatus Status { get; private set; }

    // ---- Procedure identity ---------------------------------------------------------

    /// <summary>Human-readable title of this procedure document.</summary>
    public string ProcedureTitle { get; private set; } = string.Empty;

    /// <summary>Hazardous energy source(s) this procedure addresses.</summary>
    public ProcedureEnergySourceType EnergySourceTypes { get; private set; }

    /// <summary>Narrative description of the isolation points documented by this procedure.</summary>
    public string IsolationPointDescription { get; private set; } = string.Empty;

    /// <summary>Identity of the approver.</summary>
    public UserId ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the document was published, if any.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next mandatory annual audit is due.</summary>
    public UtcTimestamp? NextAuditDueAtUtc { get; private set; }

    /// <summary>Number of authorized personnel documented under this procedure.</summary>
    public int AuthorizedPersonnelCount { get; private set; }

    /// <summary>Compliance score (0-100) from the most recent audit.</summary>
    public decimal? ComplianceScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ILotoProceduresDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ILotoProceduresDomainEvent> DomainEvents
        => new ReadOnlyCollection<ILotoProceduresDomainEvent>(_domainEvents);

    // For rehydration only.
    private LotoProcedures() { }

    // ---- Factory: Create (Draft) -----------------------------------------------------------

    /// <summary>Drafts a new LOTO procedure document and raises <see cref="LotoProcedureDraftedEvent"/>.</summary>
    public static LotoProcedures Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string procedureTitle = "",
        ProcedureEnergySourceType energySourceTypes = ProcedureEnergySourceType.Electrical,
        string isolationPointDescription = "")
    {
        var agg = new LotoProcedures
        {
            LotoProceduresId          = LotoProceduresId.New(),
            TenantId                  = tenantId,
            SiteId                    = siteId,
            Status                    = LotoProceduresStatus.Draft,
            ProcedureTitle            = (procedureTitle ?? string.Empty).Trim(),
            EnergySourceTypes         = energySourceTypes,
            IsolationPointDescription = (isolationPointDescription ?? string.Empty).Trim(),
            CreatedAtUtc              = createdAtUtc,
            CreatedBy                 = createdBy,
        };

        agg.AddDomainEvent(new LotoProcedureDraftedEvent(
            LotoProceduresEventId.New(), agg.LotoProceduresId, energySourceTypes, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: SubmitForApproval -----------------------------------------------

    /// <summary>Submits the draft for approval.</summary>
    public void SubmitForApproval(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoProceduresStatus.Draft, LotoProceduresStatus.UnderRevision);

        Status = LotoProceduresStatus.PendingApproval;

        AddDomainEvent(new LotoProcedureSubmittedForApprovalEvent(
            LotoProceduresEventId.New(), LotoProceduresId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Publish -----------------------------------------------------------

    /// <summary>Approves and publishes the procedure, documenting the authorized personnel roster.</summary>
    public void Publish(UserId approvedByUserId, int authorizedPersonnelCount, UtcTimestamp publishedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoProceduresStatus.PendingApproval);

        if (authorizedPersonnelCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(authorizedPersonnelCount), "At least one authorized employee must be documented.");

        ApprovedByUserId          = approvedByUserId;
        AuthorizedPersonnelCount  = authorizedPersonnelCount;
        PublishedAtUtc            = publishedAtUtc;
        NextAuditDueAtUtc         = new UtcTimestamp(publishedAtUtc.Value.AddYears(1));
        Status                    = LotoProceduresStatus.Published;

        AddDomainEvent(new LotoProcedurePublishedEvent(
            LotoProceduresEventId.New(), LotoProceduresId, approvedByUserId, publishedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordAnnualAudit -----------------------------------------------

    /// <summary>Records the outcome of the mandatory annual audit.</summary>
    public void RecordAnnualAudit(bool compliant, decimal complianceScore, UtcTimestamp auditedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoProceduresStatus.Published, LotoProceduresStatus.AuditFlagged);

        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        ComplianceScore   = complianceScore;
        NextAuditDueAtUtc = new UtcTimestamp(auditedAtUtc.Value.AddYears(1));

        if (compliant)
        {
            Status = LotoProceduresStatus.Published;
            AddDomainEvent(new LotoProcedureAuditPassedEvent(
                LotoProceduresEventId.New(), LotoProceduresId, complianceScore, auditedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = LotoProceduresStatus.AuditFlagged;
            AddDomainEvent(new LotoProcedureAuditFlaggedEvent(
                LotoProceduresEventId.New(), LotoProceduresId, complianceScore, auditedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: ScheduleRevision -----------------------------------------------

    /// <summary>Opens a revision cycle for the procedure.</summary>
    public void ScheduleRevision(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoProceduresStatus.Published, LotoProceduresStatus.AuditFlagged);

        Status = LotoProceduresStatus.UnderRevision;

        AddDomainEvent(new LotoProcedureRevisionScheduledEvent(
            LotoProceduresEventId.New(), LotoProceduresId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the procedure document.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == LotoProceduresStatus.Retired)
            throw new LotoProceduresInvalidStatusTransitionException(LotoProceduresId, Status.ToString(), LotoProceduresStatus.Retired.ToString(), correlationId, causationId);

        Status = LotoProceduresStatus.Retired;

        AddDomainEvent(new LotoProcedureRetiredEvent(
            LotoProceduresEventId.New(), LotoProceduresId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ILotoProceduresDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params LotoProceduresStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for LotoProcedures {LotoProceduresId}.");
    }
}
