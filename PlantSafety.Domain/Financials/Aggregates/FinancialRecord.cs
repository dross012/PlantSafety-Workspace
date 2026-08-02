// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialRecord.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Financials bounded context. Phase 4 full build-out --
//                  the Financials module previously had NO aggregate root at all, only a bare
//                  child entity (FinancialLineItem) and an empty domain-service placeholder.
//                  Tracks a single financial record (budget line, cost, or forecast entry): its
//                  category, amount, line items, and lifecycle status, grounded in the
//                  pre-existing FinancialStatus enum
//                  (Draft/Submitted/Approved/Rejected/Closed/Archived). Reuses the pre-existing
//                  FinancialLineItem entity as-is for its child collection. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Aggregates;

/// <summary>Aggregate root for the Financials bounded context.</summary>
public sealed class FinancialRecord
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this FinancialRecord aggregate.</summary>
    public FinancialsId FinancialsId { get; private set; }

    /// <summary>Tenant that owns this financial record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public FinancialStatus Status { get; private set; }

    // ---- Financial business properties ----------------------------------------

    /// <summary>Financial category (e.g. Budget, Cost, Forecast).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Monetary amount in the site's base currency.</summary>
    public decimal Amount { get; private set; }

    /// <summary>UTC timestamp the record was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>Identity of the user who recorded this financial record (CRUD authorship).</summary>
    public UserId RecordedBy { get; private set; }

    private readonly List<FinancialLineItem> _lineItems = [];

    /// <summary>Line items recorded against this financial record.</summary>
    public IReadOnlyCollection<FinancialLineItem> LineItems => new ReadOnlyCollection<FinancialLineItem>(_lineItems);

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IFinancialsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IFinancialsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IFinancialsDomainEvent>(_domainEvents);

    // For rehydration only.
    private FinancialRecord() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new FinancialRecord aggregate (Status=Draft) and raises <see cref="FinancialRecordCreatedEvent"/>.</summary>
    /// <param name="tenantId">Owning tenant. Required.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="category">Financial category (e.g. Budget, Cost, Forecast).</param>
    /// <param name="amount">Monetary amount.</param>
    /// <param name="recordedBy">CRUD authorship identity.</param>
    /// <param name="recordedAtUtc">Recording timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    public static FinancialRecord Create(
        Guid tenantId,
        Guid? siteId,
        string category,
        decimal amount,
        UserId recordedBy,
        UtcTimestamp recordedAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        var agg = new FinancialRecord
        {
            FinancialsId   = FinancialsId.New(),
            TenantId       = tenantId,
            SiteId         = siteId,
            Status         = FinancialStatus.Draft,
            Category       = (category ?? string.Empty).Trim(),
            Amount         = amount,
            RecordedAtUtc  = recordedAtUtc,
            RecordedBy     = recordedBy,
        };

        agg.AddDomainEvent(new FinancialRecordCreatedEvent(
            EventId: Uuid7.New(),
            FinancialRecordId: agg.FinancialsId.Value,
            TenantId: tenantId,
            RecordedBy: recordedBy,
            RecordedAtUtc: recordedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle -----------------------------------------------------------

    /// <summary>
    /// Appends a new line item to this financial record. Valid only while
    /// <see cref="FinancialStatus.Draft"/>.
    /// </summary>
    public void AddLineItem(
        string description,
        decimal amount,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, FinancialRecordLifecycleRules.CanAddLineItem, FinancialStatus.Draft);

        var lineItem = new FinancialLineItem(Guid.NewGuid(), (description ?? string.Empty).Trim(), amount);
        _lineItems.Add(lineItem);

        AddDomainEvent(new FinancialRecordLineItemAddedEvent(
            EventId: Uuid7.New(),
            FinancialRecordId: FinancialsId.Value,
            LineItemId: lineItem.LineItemId,
            Description: lineItem.Description,
            Amount: lineItem.Amount,
            AddedAtUtc: UtcTimestamp.Now(),
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Submits the financial record for approval (Draft -&gt; Submitted). Valid only from
    /// <see cref="FinancialStatus.Draft"/>.
    /// </summary>
    public void Submit(
        UserId submittedBy,
        UtcTimestamp submittedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, FinancialRecordLifecycleRules.CanSubmit, FinancialStatus.Draft);

        Status = FinancialStatus.Submitted;

        AddDomainEvent(new FinancialRecordSubmittedEvent(
            EventId: Uuid7.New(),
            FinancialRecordId: FinancialsId.Value,
            SubmittedBy: submittedBy,
            SubmittedAtUtc: submittedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Approves the financial record (Submitted -&gt; Approved). Valid only from
    /// <see cref="FinancialStatus.Submitted"/>.
    /// </summary>
    public void Approve(
        UserId approvedBy,
        UtcTimestamp approvedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, FinancialRecordLifecycleRules.CanApprove, FinancialStatus.Submitted);

        Status = FinancialStatus.Approved;

        AddDomainEvent(new FinancialRecordApprovedEvent(
            EventId: Uuid7.New(),
            FinancialRecordId: FinancialsId.Value,
            ApprovedBy: approvedBy,
            ApprovedAtUtc: approvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Rejects the financial record (Submitted -&gt; Rejected). Valid only from
    /// <see cref="FinancialStatus.Submitted"/>.
    /// </summary>
    public void Reject(
        UserId rejectedBy,
        string? reason,
        UtcTimestamp rejectedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, FinancialRecordLifecycleRules.CanReject, FinancialStatus.Submitted);

        Status = FinancialStatus.Rejected;

        AddDomainEvent(new FinancialRecordRejectedEvent(
            EventId: Uuid7.New(),
            FinancialRecordId: FinancialsId.Value,
            RejectedBy: rejectedBy,
            Reason: reason,
            RejectedAtUtc: rejectedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Closes the financial record (Approved -&gt; Closed). Valid only from
    /// <see cref="FinancialStatus.Approved"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, FinancialRecordLifecycleRules.CanClose, FinancialStatus.Approved);

        Status = FinancialStatus.Closed;

        AddDomainEvent(new FinancialRecordClosedEvent(
            EventId: Uuid7.New(),
            FinancialRecordId: FinancialsId.Value,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives the financial record (Closed or Rejected -&gt; Archived). Valid only from
    /// <see cref="FinancialStatus.Closed"/> or <see cref="FinancialStatus.Rejected"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!FinancialRecordLifecycleRules.CanArchive(Status))
        {
            throw new FinancialRecordInvalidStatusTransitionException(
                FinancialsId, Status.ToString(), FinancialStatus.Archived.ToString(), correlationId, causationId);
        }

        Status = FinancialStatus.Archived;

        AddDomainEvent(new FinancialRecordArchivedEvent(
            EventId: Uuid7.New(),
            FinancialRecordId: FinancialsId.Value,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IFinancialsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<FinancialStatus, bool> guard, FinancialStatus expected)
    {
        if (guard(Status)) return;

        throw new FinancialRecordInvalidStatusTransitionException(
            FinancialsId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
