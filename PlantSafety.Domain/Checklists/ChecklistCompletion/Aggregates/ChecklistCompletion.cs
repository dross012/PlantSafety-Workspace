// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletion.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a single completed or in-progress execution of a ChecklistTemplate at a site, capturing item-level pass/fail counts and overall outcome.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates;

/// <summary>Aggregate root for the ChecklistCompletion bounded context.</summary>
public sealed partial class ChecklistCompletion
{
    // ---- Identity ------------------------------------------------------

    /// <summary>Strong-typed identity of this ChecklistCompletion aggregate.</summary>
    public ChecklistCompletionId ChecklistCompletionId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ChecklistCompletionStatus Status { get; private set; }

    // ---- Business properties --------------------------------------------
    /// <summary>Identity of the ChecklistTemplate this completion was performed against.</summary>
    public Guid ChecklistTemplateId { get; private set; }

    /// <summary>Total number of checklist items evaluated during this completion.</summary>
    public int ItemsTotal { get; private set; }

    /// <summary>Number of checklist items that passed.</summary>
    public int ItemsPassed { get; private set; }

    /// <summary>Number of checklist items that failed.</summary>
    public int ItemsFailed { get; private set; }

    /// <summary>Overall outcome classification: Pending, Pass, Fail, PartialWithFindings.</summary>
    public string Outcome { get; private set; } = "Pending";

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant the execution was started, once started.</summary>
    public UtcTimestamp? StartedAtUtc { get; private set; }

    /// <summary>UTC instant the execution was completed, once completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Number of critical/stop-work item failures recorded during this completion.</summary>
    public int CriticalFailureCount { get; private set; }

    // ---- Item results --------------------------------------------------------

    private readonly List<ChecklistCompletionItemResult> _itemResults = [];

    /// <summary>Per-item pass/fail results recorded during this completion.</summary>
    public IReadOnlyCollection<ChecklistCompletionItemResult> ItemResults => new ReadOnlyCollection<ChecklistCompletionItemResult>(_itemResults);

    // ---- Domain Events ---------------------------------------------------

    private readonly List<IChecklistCompletionDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IChecklistCompletionDomainEvent> DomainEvents
        => new ReadOnlyCollection<IChecklistCompletionDomainEvent>(_domainEvents);

    // For rehydration only.
    private ChecklistCompletion() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ChecklistCompletion aggregate and raises <see cref="ChecklistCompletionCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    public static ChecklistCompletion Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid checklistTemplateId = default,
        int itemsTotal = 0,
        int itemsPassed = 0,
        int itemsFailed = 0,
        string outcome = "Pending")
    {
        var agg = new ChecklistCompletion
        {
            ChecklistCompletionId      = ChecklistCompletionId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = ChecklistCompletionStatus.Created,
            ChecklistTemplateId = checklistTemplateId,
            ItemsTotal          = itemsTotal,
            ItemsPassed         = itemsPassed,
            ItemsFailed         = itemsFailed,
            Outcome             = (outcome ?? "Pending").Trim(),
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new ChecklistCompletionCreatedEvent(
            EventId:       new ChecklistCompletionEventId(Uuid7.New()),
            ChecklistCompletionId:      agg.ChecklistCompletionId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle behavior --------------------------------------------------

    /// <summary>Starts execution of this checklist completion (Created → Active).</summary>
    /// <exception cref="ChecklistCompletionInvalidStatusTransitionException">Thrown when the completion has already been started.</exception>
    public void Start(
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != ChecklistCompletionStatus.Created)
            throw new ChecklistCompletionInvalidStatusTransitionException(
                ChecklistCompletionId, Status.ToString(), ChecklistCompletionStatus.Active.ToString(), correlationId, causationId);

        Status = ChecklistCompletionStatus.Active;
        StartedAtUtc = occurredAtUtc;

        AddDomainEvent(new ChecklistCompletionStartedEvent(
            EventId: new ChecklistCompletionEventId(Uuid7.New()),
            ChecklistCompletionId: ChecklistCompletionId,
            StartedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records the pass/fail result of a single checklist template item. Must be called while
    /// the completion is <see cref="ChecklistCompletionStatus.Active"/>.
    /// </summary>
    /// <exception cref="ChecklistCompletionInvalidStatusTransitionException">Thrown when the completion is not in progress.</exception>
    public ChecklistCompletionItemResult RecordItemResult(
        Guid checklistTemplateItemId,
        bool passed,
        string? notes,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc,
        bool isCriticalFailure = false)
    {
        if (Status != ChecklistCompletionStatus.Active)
            throw new ChecklistCompletionInvalidStatusTransitionException(
                ChecklistCompletionId, Status.ToString(), "RecordItemResult", correlationId, causationId);

        var result = new ChecklistCompletionItemResult(
            itemResultId: Guid.NewGuid(),
            checklistCompletionId: ChecklistCompletionId,
            checklistTemplateItemId: checklistTemplateItemId,
            passed: passed,
            isCriticalFailure: !passed && isCriticalFailure,
            notes: notes,
            recordedAtUtc: occurredAtUtc);

        _itemResults.Add(result);
        ItemsTotal = _itemResults.Count;
        ItemsPassed = _itemResults.Count(r => r.Passed);
        ItemsFailed = _itemResults.Count(r => !r.Passed);
        if (result.IsCriticalFailure)
            CriticalFailureCount++;

        AddDomainEvent(new ChecklistItemResultRecordedEvent(
            EventId: new ChecklistCompletionEventId(Uuid7.New()),
            ChecklistCompletionId: ChecklistCompletionId,
            ChecklistTemplateItemId: checklistTemplateItemId,
            Passed: passed,
            RecordedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return result;
    }

    /// <summary>
    /// Records a failed item result and explicitly flags it as a critical/stop-work condition
    /// (e.g. missing LOTO device, disabled interlock, failed E-Stop). Raises an additional
    /// <see cref="ChecklistItemCriticalFailureFlaggedEvent"/> beyond the standard item-result
    /// event so downstream consumers (Auditing findings, SafetyCulture Stop-Work tracking,
    /// Alerts) can react immediately.
    /// </summary>
    /// <exception cref="ChecklistCompletionInvalidStatusTransitionException">Thrown when the completion is not in progress.</exception>
    public ChecklistCompletionItemResult FlagFailedItem(
        Guid checklistTemplateItemId,
        string description,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        var result = RecordItemResult(
            checklistTemplateItemId: checklistTemplateItemId,
            passed: false,
            notes: description,
            actor: actor,
            correlationId: correlationId,
            causationId: causationId,
            occurredAtUtc: occurredAtUtc,
            isCriticalFailure: true);

        AddDomainEvent(new ChecklistItemCriticalFailureFlaggedEvent(
            EventId: new ChecklistCompletionEventId(Uuid7.New()),
            ChecklistCompletionId: ChecklistCompletionId,
            ChecklistTemplateItemId: checklistTemplateItemId,
            Description: (description ?? string.Empty).Trim(),
            FlaggedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return result;
    }

    /// <summary>
    /// Completes this checklist execution (Active → Completed), computing the overall outcome:
    /// <c>Pass</c> when every item passed, <c>Fail</c> when any critical item failed, otherwise
    /// <c>PartialWithFindings</c> when non-critical failures exist.
    /// </summary>
    /// <exception cref="ChecklistCompletionInvalidStatusTransitionException">Thrown when the completion is not in progress.</exception>
    public void Complete(
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != ChecklistCompletionStatus.Active)
            throw new ChecklistCompletionInvalidStatusTransitionException(
                ChecklistCompletionId, Status.ToString(), ChecklistCompletionStatus.Completed.ToString(), correlationId, causationId);

        Outcome = ItemsFailed == 0
            ? "Pass"
            : CriticalFailureCount > 0
                ? "Fail"
                : "PartialWithFindings";

        Status = ChecklistCompletionStatus.Completed;
        CompletedAtUtc = occurredAtUtc;

        AddDomainEvent(new ChecklistCompletionCompletedEvent(
            EventId: new ChecklistCompletionEventId(Uuid7.New()),
            ChecklistCompletionId: ChecklistCompletionId,
            Outcome: Outcome,
            ItemsTotal: ItemsTotal,
            ItemsPassed: ItemsPassed,
            ItemsFailed: ItemsFailed,
            CompletedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IChecklistCompletionDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ChecklistCompletionStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            "Invalid lifecycle transition from '" + Status + "' for ChecklistCompletion " + ChecklistCompletionId + ".");
    }
}