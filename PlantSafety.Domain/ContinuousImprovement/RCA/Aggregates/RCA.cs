// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCA.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ContinuousImprovement.RCA (Root Cause Analysis) bounded
//                  context, backing [ContinuousImprovement].[RCA]
//                  (usp_RCA_Insert / usp_RCA_Update / usp_RCA_GetById via the Domain-layer
//                  IRCARepository / RCAAggregateDapperRepository). Before this pass, this module
//                  contained only a "RCAStub.cs" placeholder marker class ("Replace with the full
//                  aggregate structure") -- there was no domain model at all backing this feature;
//                  only a read-only, date-range list query
//                  (PlantSafety.Application.ContinuousImprovement.RCA.Queries.GetRCAQuery via
//                  PlantSafety.Application.ContinuousImprovement.RCA.Repositories.IRCARepository /
//                  PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence
//                  .RCADapperRepository calling usp_RCA_GetRcaRecords) existed, and it is left
//                  entirely untouched by this pass -- it is a different, coarser reporting query,
//                  not the by-id/lifecycle path built here. Field shape (RcaId, TenantId, SiteId,
//                  ProblemStatement, Methodology, Status, InitiatedAtUtc, OwnerUserId) is grounded
//                  in that pre-existing read model's actual DTO
//                  (PlantSafety.Application.ContinuousImprovement.RCA.Models.RCAItemDto), not
//                  invented from scratch. Lifecycle states (Initiated/InProgress/PendingApproval/
//                  Closed/Archived) are taken from RCAItemDto.Status's documented vocabulary plus
//                  the platform-standard terminal Archived state. Mirrors
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates
//                  .RootCauseAnalysis's guarded-transition/domain-event/exception conventions
//                  (EnsureStatusIsOneOf + RCALifecycleRules.CanX guard pattern).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.RCA.Enums;
using PlantSafety.Domain.ContinuousImprovement.RCA.Events;
using PlantSafety.Domain.ContinuousImprovement.RCA.Exceptions;
using PlantSafety.Domain.ContinuousImprovement.RCA.Rules;
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates;

/// <summary>Aggregate root for the ContinuousImprovement.RCA (Root Cause Analysis) bounded context.</summary>
public sealed class RCA
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this RCA aggregate.</summary>
    public RCAId RcaId { get; private set; }

    /// <summary>Tenant that owns this RCA record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RCAStatus Status { get; private set; }

    // ---- RCA-specific business properties ---------------------------------

    /// <summary>Problem statement being analyzed.</summary>
    public string ProblemStatement { get; private set; } = string.Empty;

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie). Free text, not an enum.</summary>
    public string Methodology { get; private set; } = string.Empty;

    /// <summary>Identity of the user who owns the RCA.</summary>
    public UserId OwnerUserId { get; private set; }

    /// <summary>UTC timestamp when the RCA was initiated.</summary>
    public UtcTimestamp InitiatedAtUtc { get; private set; }

    // ---- Lifecycle attributes (StartAnalysis/SubmitForApproval/Close/Archive) --

    /// <summary>Identity of the user who started the analysis.</summary>
    public UserId? StartedBy { get; private set; }

    /// <summary>UTC instant analysis work began.</summary>
    public UtcTimestamp? StartedAtUtc { get; private set; }

    /// <summary>Identity of the user who submitted the RCA for approval.</summary>
    public UserId? SubmittedBy { get; private set; }

    /// <summary>Findings recorded when the RCA was submitted for approval, if any.</summary>
    public string? Findings { get; private set; }

    /// <summary>UTC instant the RCA was submitted for approval.</summary>
    public UtcTimestamp? SubmittedAtUtc { get; private set; }

    /// <summary>Identity of the user who formally closed the RCA.</summary>
    public UserId? ClosedBy { get; private set; }

    /// <summary>Root cause summary recorded when the RCA was closed, if any.</summary>
    public string? RootCauseSummary { get; private set; }

    /// <summary>UTC instant the RCA was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the RCA was archived for compliance retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IRCADomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRCADomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // For rehydration only.
    private RCA() { }

    // ---- Factory ---------------------------------------------------------------

    /// <summary>
    /// Creates a new ContinuousImprovement RCA record and raises <see cref="RCACreatedEvent"/>.
    /// Enforces the required-field invariants implied by the NOT NULL columns on
    /// [ContinuousImprovement].[RCA] (ProblemStatement, Methodology, OwnerUserId).
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static RCA Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string problemStatement,
        string methodology,
        UserId ownerUserId)
    {
        var normalizedProblemStatement = (problemStatement ?? string.Empty).Trim();
        var normalizedMethodology = (methodology ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(normalizedProblemStatement))
            throw new ArgumentException("ProblemStatement is required.", nameof(problemStatement));
        if (string.IsNullOrWhiteSpace(normalizedMethodology))
            throw new ArgumentException("Methodology is required.", nameof(methodology));
        if (ownerUserId == default)
            throw new ArgumentException("OwnerUserId is required.", nameof(ownerUserId));

        var agg = new RCA
        {
            RcaId = RCAId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = RCAStatus.Initiated,
            ProblemStatement = normalizedProblemStatement,
            Methodology = normalizedMethodology,
            OwnerUserId = ownerUserId,
            InitiatedAtUtc = createdAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new RCACreatedEvent(
            EventId: RCAEventId.New(),
            RcaId: agg.RcaId,
            TenantId: tenantId,
            SiteId: siteId,
            ProblemStatement: agg.ProblemStatement,
            Methodology: agg.Methodology,
            OwnerUserId: ownerUserId,
            InitiatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: StartAnalysis -----------------------------------------------

    /// <summary>
    /// Begins analysis work (Initiated -&gt; InProgress). Valid only from
    /// <see cref="RCAStatus.Initiated"/>.
    /// </summary>
    public void StartAnalysis(
        UserId startedBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RCALifecycleRules.CanStartAnalysis, RCAStatus.InProgress);

        StartedBy = startedBy;
        StartedAtUtc = startedAtUtc;
        Status = RCAStatus.InProgress;

        AddDomainEvent(new RCAAnalysisStartedEvent(
            EventId: RCAEventId.New(),
            RcaId: RcaId,
            StartedBy: startedBy,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: SubmitForApproval --------------------------------------------

    /// <summary>
    /// Submits the RCA for approval (InProgress -&gt; PendingApproval). Valid only from
    /// <see cref="RCAStatus.InProgress"/>.
    /// </summary>
    public void SubmitForApproval(
        UserId submittedBy,
        string? findings,
        UtcTimestamp submittedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RCALifecycleRules.CanSubmitForApproval, RCAStatus.PendingApproval);

        SubmittedBy = submittedBy;
        Findings = string.IsNullOrWhiteSpace(findings) ? null : findings.Trim();
        SubmittedAtUtc = submittedAtUtc;
        Status = RCAStatus.PendingApproval;

        AddDomainEvent(new RCASubmittedForApprovalEvent(
            EventId: RCAEventId.New(),
            RcaId: RcaId,
            SubmittedBy: submittedBy,
            Findings: Findings,
            SubmittedAtUtc: submittedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close ---------------------------------------------------------

    /// <summary>
    /// Approves and formally closes the RCA (PendingApproval -&gt; Closed). Valid only from
    /// <see cref="RCAStatus.PendingApproval"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        string? rootCauseSummary,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RCALifecycleRules.CanClose, RCAStatus.Closed);

        ClosedBy = closedBy;
        RootCauseSummary = string.IsNullOrWhiteSpace(rootCauseSummary) ? null : rootCauseSummary.Trim();
        ClosedAtUtc = closedAtUtc;
        Status = RCAStatus.Closed;

        AddDomainEvent(new RCAClosedEvent(
            EventId: RCAEventId.New(),
            RcaId: RcaId,
            ClosedBy: closedBy,
            RootCauseSummary: RootCauseSummary,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive ---------------------------------------------------------

    /// <summary>
    /// Archives a closed RCA for compliance retention (Closed -&gt; Archived). Valid only from
    /// <see cref="RCAStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RCALifecycleRules.CanArchive, RCAStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = RCAStatus.Archived;

        AddDomainEvent(new RCAArchivedEvent(
            EventId: RCAEventId.New(),
            RcaId: RcaId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRCADomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<RCAStatus, bool> guard, RCAStatus expected)
    {
        if (guard(Status)) return;

        throw new RCAInvalidStatusTransitionException(RcaId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
