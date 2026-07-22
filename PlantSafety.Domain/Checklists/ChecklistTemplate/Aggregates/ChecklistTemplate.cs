// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplate.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a single checklist template/category definition (e.g. "Machine Guarding and LOTO Checklists") used to standardize recurring plant-floor safety inspections.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates;

/// <summary>Aggregate root for the ChecklistTemplate bounded context.</summary>
public sealed partial class ChecklistTemplate
{
    // ---- Identity ------------------------------------------------------

    /// <summary>Strong-typed identity of this ChecklistTemplate aggregate.</summary>
    public ChecklistTemplateId ChecklistTemplateId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ChecklistTemplateStatus Status { get; private set; }

    // ---- Business properties --------------------------------------------
    /// <summary>Human-readable checklist template title.</summary>
    public string TemplateTitle { get; private set; } = string.Empty;

    /// <summary>Checklist category code (see ChecklistCategory smart enum).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Narrative description of what this checklist template covers.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Number of checklist items defined on this template.</summary>
    public int ItemCount { get; private set; }

    /// <summary>Whether completion of this checklist is mandatory (vs. optional/best-practice).</summary>
    public bool IsMandatory { get; private set; }

    /// <summary>Optional regulatory citation this checklist supports (e.g. "OSHA 29 CFR 1910.147").</summary>
    public string? RegulatoryReference { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant the template was published (Created → Active), if published.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>UTC instant the template was retired, if retired.</summary>
    public UtcTimestamp? RetiredAtUtc { get; private set; }

    /// <summary>Reason recorded when the template was retired, if retired.</summary>
    public string? RetirementReason { get; private set; }

    // ---- Line items --------------------------------------------------------

    private readonly List<ChecklistTemplateItem> _items = [];

    /// <summary>Inspection line items defined on this template, in sequence order.</summary>
    public IReadOnlyCollection<ChecklistTemplateItem> Items => new ReadOnlyCollection<ChecklistTemplateItem>(_items);

    // ---- Domain Events ---------------------------------------------------

    private readonly List<IChecklistTemplateDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IChecklistTemplateDomainEvent> DomainEvents
        => new ReadOnlyCollection<IChecklistTemplateDomainEvent>(_domainEvents);

    // For rehydration only.
    private ChecklistTemplate() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ChecklistTemplate aggregate and raises <see cref="ChecklistTemplateCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    public static ChecklistTemplate Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string templateTitle = "",
        string category = "",
        string description = "",
        int itemCount = 0,
        bool isMandatory = false,
        string? regulatoryReference = null)
    {
        var agg = new ChecklistTemplate
        {
            ChecklistTemplateId      = ChecklistTemplateId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = ChecklistTemplateStatus.Created,
            TemplateTitle       = (templateTitle ?? string.Empty).Trim(),
            Category            = (category ?? string.Empty).Trim(),
            Description         = (description ?? string.Empty).Trim(),
            ItemCount           = itemCount,
            IsMandatory         = isMandatory,
            RegulatoryReference = regulatoryReference,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new ChecklistTemplateCreatedEvent(
            EventId:       new ChecklistTemplateEventId(Uuid7.New()),
            ChecklistTemplateId:      agg.ChecklistTemplateId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle behavior --------------------------------------------------

    /// <summary>
    /// Adds a new inspection line item to this template. Only permitted while the template is
    /// in <see cref="ChecklistTemplateStatus.Created"/> (draft) status -- items cannot be added
    /// after publication to keep executed <see cref="Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion"/>
    /// records comparable against a stable definition.
    /// </summary>
    /// <param name="description">Short inspection instruction text.</param>
    /// <param name="isCritical">Whether a failed answer on this item is a stop-work/critical condition.</param>
    /// <param name="regulatoryReference">Optional item-level regulatory citation.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="occurredAtUtc">UTC instant of this action.</param>
    /// <exception cref="ChecklistTemplateInvalidStatusTransitionException">Thrown when the template is not in draft status.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="description"/> is empty.</exception>
    public ChecklistTemplateItem AddItem(
        string description,
        bool isCritical,
        string? regulatoryReference,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != ChecklistTemplateStatus.Created)
            throw new ChecklistTemplateInvalidStatusTransitionException(
                ChecklistTemplateId, Status.ToString(), "ItemAdd", correlationId, causationId);

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Checklist item description must not be empty.", nameof(description));

        var item = new ChecklistTemplateItem(
            itemId: Guid.NewGuid(),
            checklistTemplateId: ChecklistTemplateId,
            sequence: _items.Count + 1,
            description: description,
            isCritical: isCritical,
            regulatoryReference: regulatoryReference,
            addedAtUtc: occurredAtUtc);

        _items.Add(item);
        ItemCount = _items.Count;

        AddDomainEvent(new ChecklistTemplateItemAddedEvent(
            EventId: new ChecklistTemplateEventId(Uuid7.New()),
            ChecklistTemplateId: ChecklistTemplateId,
            ItemId: item.ItemId,
            Sequence: item.Sequence,
            IsCritical: item.IsCritical,
            AddedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return item;
    }

    /// <summary>
    /// Publishes this template (Created → Active), making it available for
    /// <see cref="Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion"/> executions.
    /// Requires at least one line item to have been added.
    /// </summary>
    /// <exception cref="ChecklistTemplateInvalidStatusTransitionException">Thrown when the template is not in draft status.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the template has no line items.</exception>
    public void Publish(
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != ChecklistTemplateStatus.Created)
            throw new ChecklistTemplateInvalidStatusTransitionException(
                ChecklistTemplateId, Status.ToString(), ChecklistTemplateStatus.Active.ToString(), correlationId, causationId);

        if (_items.Count == 0)
            throw new InvalidOperationException(
                "Cannot publish ChecklistTemplate " + ChecklistTemplateId + " with no line items.");

        Status = ChecklistTemplateStatus.Active;
        PublishedAtUtc = occurredAtUtc;

        AddDomainEvent(new ChecklistTemplatePublishedEvent(
            EventId: new ChecklistTemplateEventId(Uuid7.New()),
            ChecklistTemplateId: ChecklistTemplateId,
            ItemCount: _items.Count,
            PublishedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Retires this template (Active/Completed → Archived), removing it from active circulation.
    /// Existing <see cref="Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion"/> history remains valid.
    /// </summary>
    /// <param name="reason">Narrative reason for retirement (e.g. "Superseded by 2027 revision").</param>
    /// <exception cref="ChecklistTemplateInvalidStatusTransitionException">Thrown when the template is still in draft or already archived.</exception>
    public void Retire(
        string reason,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        EnsureStatusIsOneOf(ChecklistTemplateStatus.Active, ChecklistTemplateStatus.Completed);

        Status = ChecklistTemplateStatus.Archived;
        RetiredAtUtc = occurredAtUtc;
        RetirementReason = (reason ?? string.Empty).Trim();

        AddDomainEvent(new ChecklistTemplateRetiredEvent(
            EventId: new ChecklistTemplateEventId(Uuid7.New()),
            ChecklistTemplateId: ChecklistTemplateId,
            Reason: RetirementReason,
            RetiredAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IChecklistTemplateDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ChecklistTemplateStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            "Invalid lifecycle transition from '" + Status + "' for ChecklistTemplate " + ChecklistTemplateId + ".");
    }
}