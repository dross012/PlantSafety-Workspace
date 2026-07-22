// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflow.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PermitWorkflow bounded context (module: PermitToWork).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates;

/// <summary>Aggregate root for the PermitWorkflow bounded context.</summary>
public sealed partial class PermitWorkflow
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PermitWorkflow aggregate.</summary>
    public PermitWorkflowId PermitWorkflowId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitWorkflowStatus Status { get; private set; }

    // ---- PermitWorkflow-specific business properties --------------------------------

    /// <summary>PermitNumber business attribute.</summary>
    public string PermitNumber { get; private set; }

    /// <summary>PermitTypeCode business attribute.</summary>
    public string PermitTypeCode { get; private set; }

    /// <summary>RequesterUserId business attribute.</summary>
    public UserId? RequesterUserId { get; private set; }

    /// <summary>IssuerUserId business attribute.</summary>
    public UserId? IssuerUserId { get; private set; }

    /// <summary>WorkDescription business attribute.</summary>
    public string WorkDescription { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- PTW workflow lifecycle attributes (Steps 2-7) ----------------------

    /// <summary>Required controls (LOTO, PPE, ventilation, fire watch, barricades, atmospheric testing) identified during hazard assessment (Step 2).</summary>
    public string? RequiredControls { get; private set; }

    /// <summary>Risk score assigned during hazard assessment.</summary>
    public int? RiskScore { get; private set; }

    /// <summary>Free-text confirmation of who implemented the required controls (Step 3).</summary>
    public string? ControlsConfirmedBy { get; private set; }

    /// <summary>UTC instant the required controls were confirmed implemented.</summary>
    public UtcTimestamp? ControlsImplementedAtUtc { get; private set; }

    /// <summary>Identity of the supervisor/safety officer who approved the permit (Step 4).</summary>
    public UserId? ApprovedBy { get; private set; }

    /// <summary>UTC instant the permit was approved.</summary>
    public UtcTimestamp? ApprovedAtUtc { get; private set; }

    /// <summary>UTC instant the permit was issued and became active (Step 4/5).</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>UTC instant after which an active permit must be closed or renewed.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>Identity of the person who signed the permit closure (Step 6).</summary>
    public UserId? ClosureSignedBy { get; private set; }

    /// <summary>Free-text notes captured at closure (post-work review findings, issues logged for corrective action).</summary>
    public string? ClosureNotes { get; private set; }

    /// <summary>UTC instant the permit was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the permit was archived for compliance retention (Step 7).</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>Reason recorded when a permit request is rejected.</summary>
    public string? RejectedReason { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitWorkflowDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitWorkflowDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitWorkflowDomainEvent>(_domainEvents);

    // For rehydration only.
    private PermitWorkflow() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PermitWorkflow aggregate and raises <see cref="PermitWorkflowCreatedEvent"/>.</summary>
    public static PermitWorkflow Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitNumber = "",
        string permitTypeCode = "",
        UserId? requesterUserId = null,
        UserId? issuerUserId = null,
        string workDescription = "")
    {
        var agg = new PermitWorkflow
        {
            PermitWorkflowId     = PermitWorkflowId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PermitWorkflowStatus.Requested,
            PermitNumber = (permitNumber ?? string.Empty).Trim(),
            PermitTypeCode = (permitTypeCode ?? string.Empty).Trim(),
            RequesterUserId = requesterUserId,
            IssuerUserId = issuerUserId,
            WorkDescription = (workDescription ?? string.Empty).Trim(),
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PermitWorkflowCreatedEvent(
            EventId:       new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId:      agg.PermitWorkflowId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: PTW Workflow Step 2 - Hazard Assessment -----------------

    /// <summary>
    /// Records the safety officer's hazard assessment and the required controls that must be
    /// implemented before work may begin (PTW Workflow Step 2). Valid only from <see cref="PermitWorkflowStatus.Requested"/>.
    /// </summary>
    public void AssessHazards(
        string requiredControls,
        int riskScore,
        UtcTimestamp assessedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitWorkflowStatus.Requested);
        if (string.IsNullOrWhiteSpace(requiredControls))
            throw new ArgumentException("RequiredControls must be supplied for a hazard assessment.", nameof(requiredControls));
        if (riskScore < 0)
            throw new ArgumentOutOfRangeException(nameof(riskScore), riskScore, "RiskScore must be non-negative.");

        RequiredControls = requiredControls.Trim();
        RiskScore = riskScore;
        Status = PermitWorkflowStatus.HazardAssessed;

        AddDomainEvent(new PermitWorkflowHazardsAssessedEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            RequiredControls: RequiredControls,
            RiskScore: riskScore,
            AssessedAtUtc: assessedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: PTW Workflow Step 3 - Control Implementation ------------

    /// <summary>
    /// Records that the required safety controls (LOTO, PPE, barricades, atmospheric testing,
    /// etc.) have been implemented (PTW Workflow Step 3). Valid only from <see cref="PermitWorkflowStatus.HazardAssessed"/>.
    /// </summary>
    public void ImplementControls(
        string controlsConfirmedBy,
        UtcTimestamp implementedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitWorkflowStatus.HazardAssessed);
        if (string.IsNullOrWhiteSpace(controlsConfirmedBy))
            throw new ArgumentException("ControlsConfirmedBy must be supplied.", nameof(controlsConfirmedBy));

        ControlsConfirmedBy = controlsConfirmedBy.Trim();
        ControlsImplementedAtUtc = implementedAtUtc;
        Status = PermitWorkflowStatus.ControlsImplemented;

        AddDomainEvent(new PermitWorkflowControlsImplementedEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            ControlsConfirmedBy: ControlsConfirmedBy,
            ImplementedAtUtc: implementedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: PTW Workflow Step 4 - Authorization ----------------------

    /// <summary>
    /// Records supervisor/safety-officer approval of the permit (PTW Workflow Step 4).
    /// Valid only from <see cref="PermitWorkflowStatus.ControlsImplemented"/>.
    /// </summary>
    public void Approve(
        UserId approvedBy,
        UtcTimestamp approvedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitWorkflowStatus.ControlsImplemented);

        ApprovedBy = approvedBy;
        ApprovedAtUtc = approvedAtUtc;
        Status = PermitWorkflowStatus.Approved;

        AddDomainEvent(new PermitWorkflowApprovedEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            ApprovedBy: approvedBy,
            ApprovedAtUtc: approvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Issues the approved permit, making it active in the field (PTW Workflow Step 4/5).
    /// Valid only from <see cref="PermitWorkflowStatus.Approved"/>.
    /// </summary>
    public void Issue(
        UserId issuerUserId,
        UtcTimestamp issuedAtUtc,
        int? validityHours,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitWorkflowStatus.Approved);

        IssuerUserId = issuerUserId;
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = validityHours is > 0
            ? UtcTimestamp.From(issuedAtUtc.Value.AddHours(validityHours.Value))
            : null;
        Status = PermitWorkflowStatus.Active;

        AddDomainEvent(new PermitWorkflowIssuedEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            IssuerUserId: issuerUserId,
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: ExpiresAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: PTW Workflow Step 6 - Close-Out --------------------------

    /// <summary>
    /// Records that the work area has been inspected, LOTO removed/equipment restored, and the
    /// closure signature obtained, closing the permit (PTW Workflow Step 6).
    /// Valid only from <see cref="PermitWorkflowStatus.Active"/>.
    /// </summary>
    public void RecordClosureSignature(
        UserId closureSignedBy,
        string? closureNotes,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitWorkflowStatus.Active);

        ClosureSignedBy = closureSignedBy;
        ClosureNotes = string.IsNullOrWhiteSpace(closureNotes) ? null : closureNotes.Trim();
        ClosedAtUtc = closedAtUtc;
        Status = PermitWorkflowStatus.Closed;

        AddDomainEvent(new PermitWorkflowClosedEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            ClosureSignedBy: closureSignedBy,
            ClosureNotes: ClosureNotes,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: PTW Workflow Step 7 - Review & Archive -------------------

    /// <summary>
    /// Archives a closed permit for compliance review and long-term retention (PTW Workflow Step 7).
    /// Valid only from <see cref="PermitWorkflowStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitWorkflowStatus.Closed);

        ArchivedAtUtc = archivedAtUtc;
        Status = PermitWorkflowStatus.Archived;

        AddDomainEvent(new PermitWorkflowArchivedEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Rejection & Expiration -----------------------------------

    /// <summary>
    /// Declines a permit request during hazard assessment, control implementation, or
    /// authorization review. Valid from <see cref="PermitWorkflowStatus.Requested"/>,
    /// <see cref="PermitWorkflowStatus.HazardAssessed"/>, <see cref="PermitWorkflowStatus.ControlsImplemented"/>,
    /// or <see cref="PermitWorkflowStatus.Approved"/>.
    /// </summary>
    public void Reject(
        UserId rejectedBy,
        string reason,
        UtcTimestamp rejectedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(
            correlationId, causationId,
            PermitWorkflowStatus.Requested,
            PermitWorkflowStatus.HazardAssessed,
            PermitWorkflowStatus.ControlsImplemented,
            PermitWorkflowStatus.Approved);

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A rejection reason must be supplied.", nameof(reason));

        RejectedReason = reason.Trim();
        Status = PermitWorkflowStatus.Rejected;

        AddDomainEvent(new PermitWorkflowRejectedEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            RejectedBy: rejectedBy,
            Reason: RejectedReason,
            RejectedAtUtc: rejectedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Automatically expires an active permit that passed its validity period without being
    /// closed. Valid only from <see cref="PermitWorkflowStatus.Active"/>.
    /// </summary>
    public void Expire(
        UtcTimestamp expiredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitWorkflowStatus.Active);

        Status = PermitWorkflowStatus.Expired;

        AddDomainEvent(new PermitWorkflowExpiredEvent(
            EventId: new PermitWorkflowEventId(Uuid7.New()),
            PermitWorkflowId: PermitWorkflowId,
            ExpiredAtUtc: expiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitWorkflowDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitWorkflowStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PermitWorkflow {PermitWorkflowId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params PermitWorkflowStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new PermitWorkflowInvalidStatusTransitionException(
            PermitWorkflowId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}