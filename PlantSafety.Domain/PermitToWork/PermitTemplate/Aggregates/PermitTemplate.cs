// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplate.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PermitTemplate bounded context (module: PermitToWork).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates;

/// <summary>Aggregate root for the PermitTemplate bounded context.</summary>
public sealed partial class PermitTemplate
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PermitTemplate aggregate.</summary>
    public PermitTemplateId PermitTemplateId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitTemplateStatus Status { get; private set; }

    // ---- PermitTemplate-specific business properties --------------------------------

    /// <summary>PermitTypeCode business attribute.</summary>
    public string PermitTypeCode { get; private set; }

    /// <summary>RequiredControls business attribute.</summary>
    public string RequiredControls { get; private set; }

    /// <summary>StandardValidityHours business attribute.</summary>
    public int StandardValidityHours { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitTemplateDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitTemplateDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitTemplateDomainEvent>(_domainEvents);

    // For rehydration only.
    private PermitTemplate() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PermitTemplate aggregate and raises <see cref="PermitTemplateCreatedEvent"/>.</summary>
    public static PermitTemplate Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitTypeCode = "",
        string requiredControls = "",
        int standardValidityHours = 0)
    {
        var agg = new PermitTemplate
        {
            PermitTemplateId     = PermitTemplateId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PermitTemplateStatus.Draft,
            PermitTypeCode = (permitTypeCode ?? string.Empty).Trim(),
            RequiredControls = (requiredControls ?? string.Empty).Trim(),
            StandardValidityHours = standardValidityHours,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PermitTemplateCreatedEvent(
            EventId:       new PermitTemplateEventId(Uuid7.New()),
            PermitTemplateId:      agg.PermitTemplateId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle -----------------------------------------------------------

    /// <summary>Publishes a draft template, making it available for permit requests of this category. Valid only from <see cref="PermitTemplateStatus.Draft"/>.</summary>
    public void Publish(UtcTimestamp publishedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitTemplateStatus.Draft);

        Status = PermitTemplateStatus.Active;

        AddDomainEvent(new PermitTemplatePublishedEvent(
            EventId: new PermitTemplateEventId(Uuid7.New()),
            PermitTemplateId: PermitTemplateId,
            PublishedAtUtc: publishedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Deprecates an active template: in-flight permits may complete, but no new permits may be requested against it. Valid only from <see cref="PermitTemplateStatus.Active"/>.</summary>
    public void Deprecate(string reason, UtcTimestamp deprecatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitTemplateStatus.Active);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A deprecation reason must be supplied.", nameof(reason));

        Status = PermitTemplateStatus.Deprecated;

        AddDomainEvent(new PermitTemplateDeprecatedEvent(
            EventId: new PermitTemplateEventId(Uuid7.New()),
            PermitTemplateId: PermitTemplateId,
            Reason: reason.Trim(),
            DeprecatedAtUtc: deprecatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Retires a deprecated template so it is no longer usable for any purpose. Valid only from <see cref="PermitTemplateStatus.Deprecated"/>.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitTemplateStatus.Deprecated);

        Status = PermitTemplateStatus.Retired;

        AddDomainEvent(new PermitTemplateRetiredEvent(
            EventId: new PermitTemplateEventId(Uuid7.New()),
            PermitTemplateId: PermitTemplateId,
            RetiredAtUtc: retiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives a retired template for long-term compliance retention. Valid only from <see cref="PermitTemplateStatus.Retired"/>.</summary>
    public void Archive(UtcTimestamp archivedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitTemplateStatus.Retired);

        Status = PermitTemplateStatus.Archived;

        AddDomainEvent(new PermitTemplateArchivedEvent(
            EventId: new PermitTemplateEventId(Uuid7.New()),
            PermitTemplateId: PermitTemplateId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitTemplateDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitTemplateStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PermitTemplate {PermitTemplateId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params PermitTemplateStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new PermitTemplateInvalidStatusTransitionException(
            PermitTemplateId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}