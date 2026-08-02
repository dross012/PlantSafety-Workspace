// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagement.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SafetyEvents.CrisisManagement bounded context. Tracks a
//                  single declared crisis from declaration through response, stabilization,
//                  resolution, closure, and archival.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at
//                  all -- only a placeholder marker class (CrisisManagementModuleMarker) and a
//                  read-only list query slice over the [SafetyEvents].[CrisisManagement] table
//                  existed. Field shape (Title, Severity, Status, DeclaredAtUtc, DeclaredBy) is
//                  grounded in that pre-existing read model's actual columns, not invented from
//                  scratch. Mirrors PlantSafety.Domain.Compliance.Permits.Aggregates.Permits's
//                  guarded-transition/domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Enums;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Events;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Exceptions;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Rules;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Aggregates;

/// <summary>Aggregate root for the SafetyEvents.CrisisManagement bounded context.</summary>
public sealed class CrisisManagement
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CrisisManagement aggregate.</summary>
    public CrisisManagementId CrisisManagementId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CrisisManagementStatus Status { get; private set; }

    // ---- Crisis-specific business properties -----------------------------------

    /// <summary>Short crisis title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public string Severity { get; private set; } = string.Empty;

    /// <summary>UTC instant the crisis was declared.</summary>
    public UtcTimestamp DeclaredAtUtc { get; private set; }

    // ---- Lifecycle attributes (RespondTo/Stabilize/Resolve/Close/Archive) ------

    /// <summary>Identity of the user who responded to the crisis.</summary>
    public UserId? RespondedBy { get; private set; }

    /// <summary>UTC instant the crisis was responded to.</summary>
    public UtcTimestamp? RespondedAtUtc { get; private set; }

    /// <summary>UTC instant the crisis was reported stabilized.</summary>
    public UtcTimestamp? StabilizedAtUtc { get; private set; }

    /// <summary>Identity of the user who resolved the crisis.</summary>
    public UserId? ResolvedBy { get; private set; }

    /// <summary>UTC instant the crisis was resolved.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

    /// <summary>Identity of the user who formally closed the crisis.</summary>
    public UserId? ClosedBy { get; private set; }

    /// <summary>UTC instant the crisis was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the crisis record was archived.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICrisisManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICrisisManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICrisisManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private CrisisManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Declares a new CrisisManagement crisis and records <see cref="CrisisManagementDeclaredEvent"/>.</summary>
    public static CrisisManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string title,
        string severity,
        UtcTimestamp declaredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must be supplied.", nameof(title));
        if (string.IsNullOrWhiteSpace(severity))
            throw new ArgumentException("Severity must be supplied.", nameof(severity));

        var agg = new CrisisManagement
        {
            CrisisManagementId = CrisisManagementId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = CrisisManagementStatus.Declared,
            Title = title.Trim(),
            Severity = severity.Trim(),
            DeclaredAtUtc = declaredAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new CrisisManagementDeclaredEvent(
            EventId: CrisisManagementEventId.New(),
            CrisisManagementId: agg.CrisisManagementId,
            TenantId: tenantId,
            SiteId: siteId,
            Title: agg.Title,
            Severity: agg.Severity,
            DeclaredAtUtc: declaredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: RespondTo ------------------------------------------------

    /// <summary>
    /// Records a response to the declared crisis (Declared -&gt; Active). Valid only from
    /// <see cref="CrisisManagementStatus.Declared"/>.
    /// </summary>
    public void RespondTo(
        UserId respondedBy,
        UtcTimestamp respondedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CrisisManagementLifecycleRules.CanRespondTo, CrisisManagementStatus.Active);

        RespondedBy = respondedBy;
        RespondedAtUtc = respondedAtUtc;
        Status = CrisisManagementStatus.Active;

        AddDomainEvent(new CrisisManagementRespondedToEvent(
            EventId: CrisisManagementEventId.New(),
            CrisisManagementId: CrisisManagementId,
            RespondedBy: respondedBy,
            RespondedAtUtc: respondedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Stabilize -------------------------------------------------

    /// <summary>
    /// Records that an active crisis has been stabilized. Remains <see cref="CrisisManagementStatus.Active"/>.
    /// Valid only while <see cref="CrisisManagementStatus.Active"/>.
    /// </summary>
    public void Stabilize(
        UserId stabilizedBy,
        UtcTimestamp stabilizedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CrisisManagementLifecycleRules.CanStabilize, CrisisManagementStatus.Active);

        StabilizedAtUtc = stabilizedAtUtc;

        AddDomainEvent(new CrisisManagementStabilizedEvent(
            EventId: CrisisManagementEventId.New(),
            CrisisManagementId: CrisisManagementId,
            StabilizedBy: stabilizedBy,
            StabilizedAtUtc: stabilizedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Resolve -----------------------------------------------------

    /// <summary>
    /// Resolves an active crisis (Active -&gt; Completed). Valid only from
    /// <see cref="CrisisManagementStatus.Active"/>.
    /// </summary>
    public void Resolve(
        UserId resolvedBy,
        UtcTimestamp resolvedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CrisisManagementLifecycleRules.CanResolve, CrisisManagementStatus.Completed);

        ResolvedBy = resolvedBy;
        ResolvedAtUtc = resolvedAtUtc;
        Status = CrisisManagementStatus.Completed;

        AddDomainEvent(new CrisisManagementResolvedEvent(
            EventId: CrisisManagementEventId.New(),
            CrisisManagementId: CrisisManagementId,
            ResolvedBy: resolvedBy,
            ResolvedAtUtc: resolvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close ------------------------------------------------------

    /// <summary>
    /// Formally closes a resolved crisis (Completed -&gt; Closed). Valid only from
    /// <see cref="CrisisManagementStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CrisisManagementLifecycleRules.CanClose, CrisisManagementStatus.Closed);

        ClosedBy = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = CrisisManagementStatus.Closed;

        AddDomainEvent(new CrisisManagementClosedEvent(
            EventId: CrisisManagementEventId.New(),
            CrisisManagementId: CrisisManagementId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive -----------------------------------------------------

    /// <summary>
    /// Archives a closed crisis record (Closed -&gt; Archived). Valid only from
    /// <see cref="CrisisManagementStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CrisisManagementLifecycleRules.CanArchive, CrisisManagementStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = CrisisManagementStatus.Archived;

        AddDomainEvent(new CrisisManagementArchivedEvent(
            EventId: CrisisManagementEventId.New(),
            CrisisManagementId: CrisisManagementId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICrisisManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<CrisisManagementStatus, bool> guard, CrisisManagementStatus expected)
    {
        if (guard(Status)) return;

        throw new CrisisManagementInvalidStatusTransitionException(
            CrisisManagementId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
