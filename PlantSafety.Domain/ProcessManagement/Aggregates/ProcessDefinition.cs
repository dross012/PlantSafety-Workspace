// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinition.cs
//  Created:        2026-07-15
//  Updated:        2026-08-02 -- Phase 4 full build-out: converted the string Status field to the
//                  documented ProcessDefinitionStatus enum (additive-only, field name unchanged)
//                  and added the SubmitForReview/Approve/Retire/Archive/Revise lifecycle. Mirrors
//                  Compliance.Policy's Supersede-style same-status action for Revise.
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ProcessManagement bounded context. Represents a
//                  documented operational/manufacturing process definition (its safety-critical
//                  parameters, revision, owner, and lifecycle status) tracked per tenant/site.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Aggregates;

/// <summary>
/// Aggregate root for the ProcessManagement bounded context. Tracks a single manufacturing or
/// operational process definition, its safety-critical category, workflow status, and owner.
/// </summary>
public sealed class ProcessDefinition
{
    /// <summary>Unique identity of this ProcessDefinition aggregate.</summary>
    public ProcessDefinitionId ProcessDefinitionId { get; private set; }

    /// <summary>Tenant that owns this process definition.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this process is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Human-readable process name (e.g., "Confined Space Entry — Tank Farm B").</summary>
    public string ProcessName { get; private set; } = string.Empty;

    /// <summary>Safety-critical process category (e.g., HazardousMaterials, ConfinedSpace, LOTO, HotWork).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public ProcessDefinitionStatus Status { get; private set; } = ProcessDefinitionStatus.Draft;

    /// <summary>Monotonically increasing revision number, starting at 1.</summary>
    public int Version { get; private set; } = 1;

    /// <summary>Identity of the process owner (accountable role holder).</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IProcessDefinitionDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IProcessDefinitionDomainEvent> DomainEvents
        => new ReadOnlyCollection<IProcessDefinitionDomainEvent>(_domainEvents);

    private ProcessDefinition() { }

    /// <summary>Creates a new ProcessDefinition aggregate in Draft status and raises <see cref="ProcessDefinitionCreatedEvent"/>.</summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="processName">Human-readable process name. Must not be null or whitespace.</param>
    /// <param name="category">Safety-critical process category. Must not be null or whitespace.</param>
    /// <param name="ownerId">Accountable process owner.</param>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="processName"/> or <paramref name="category"/> is null or whitespace.</exception>
    public static ProcessDefinition Create(
        Guid tenantId, Guid? siteId, string processName, string category, UserId ownerId,
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(processName))
            throw new ArgumentException("ProcessName must not be null or whitespace.", nameof(processName));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category must not be null or whitespace.", nameof(category));

        var agg = new ProcessDefinition
        {
            ProcessDefinitionId = ProcessDefinitionId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            ProcessName         = processName.Trim(),
            Category            = category.Trim(),
            Status              = ProcessDefinitionStatus.Draft,
            Version             = 1,
            OwnerId             = ownerId,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg._domainEvents.Add(new ProcessDefinitionCreatedEvent(
            EventId:             Uuid7.New(),
            ProcessDefinitionId: agg.ProcessDefinitionId.Value,
            TenantId:            tenantId,
            ProcessName:         agg.ProcessName,
            Category:            agg.Category,
            CreatedAtUtc:        createdAtUtc,
            Actor:               actor,
            CorrelationId:       correlationId,
            CausationId:         causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    // ---- Lifecycle: SubmitForReview / Approve / Retire / Archive / Revise --

    /// <summary>
    /// Submits this Draft process definition for review (Draft -&gt; UnderReview). Valid only from
    /// <see cref="ProcessDefinitionStatus.Draft"/>.
    /// </summary>
    public void SubmitForReview(
        UserId submittedBy,
        UtcTimestamp submittedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProcessDefinitionLifecycleRules.CanSubmitForReview, ProcessDefinitionStatus.Draft);

        Status = ProcessDefinitionStatus.UnderReview;

        AddDomainEvent(new ProcessDefinitionSubmittedForReviewEvent(
            EventId: Uuid7.New(),
            ProcessDefinitionId: ProcessDefinitionId.Value,
            SubmittedByUserId: submittedBy.Value,
            SubmittedAtUtc: submittedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Approves a process definition under review and makes it effective (UnderReview -&gt; Active).
    /// Valid only from <see cref="ProcessDefinitionStatus.UnderReview"/>.
    /// </summary>
    public void Approve(
        UserId approvedBy,
        UtcTimestamp approvedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProcessDefinitionLifecycleRules.CanApprove, ProcessDefinitionStatus.UnderReview);

        Status = ProcessDefinitionStatus.Active;

        AddDomainEvent(new ProcessDefinitionApprovedEvent(
            EventId: Uuid7.New(),
            ProcessDefinitionId: ProcessDefinitionId.Value,
            ApprovedByUserId: approvedBy.Value,
            ApprovedAtUtc: approvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Retires an active process definition from use (Active -&gt; Retired). Valid only from
    /// <see cref="ProcessDefinitionStatus.Active"/>.
    /// </summary>
    public void Retire(
        UserId retiredBy,
        string? reason,
        UtcTimestamp retiredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProcessDefinitionLifecycleRules.CanRetire, ProcessDefinitionStatus.Active);

        Status = ProcessDefinitionStatus.Retired;

        AddDomainEvent(new ProcessDefinitionRetiredEvent(
            EventId: Uuid7.New(),
            ProcessDefinitionId: ProcessDefinitionId.Value,
            RetiredByUserId: retiredBy.Value,
            Reason: reason,
            RetiredAtUtc: retiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a retired process definition for compliance retention (Retired -&gt; Archived). Valid
    /// only from <see cref="ProcessDefinitionStatus.Retired"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProcessDefinitionLifecycleRules.CanArchive, ProcessDefinitionStatus.Retired);

        Status = ProcessDefinitionStatus.Archived;

        AddDomainEvent(new ProcessDefinitionArchivedEvent(
            EventId: Uuid7.New(),
            ProcessDefinitionId: ProcessDefinitionId.Value,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Revises an active process definition in place (same-status action; Active remains Active),
    /// incrementing <see cref="Version"/>. Valid only from <see cref="ProcessDefinitionStatus.Active"/>.
    /// Mirrors Compliance.Policy's Supersede-style / Analytics.Dashboard's RefreshData-style
    /// same-status action pattern.
    /// </summary>
    public void Revise(
        UserId revisedBy,
        UtcTimestamp revisedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProcessDefinitionLifecycleRules.CanRevise, ProcessDefinitionStatus.Active);

        Version++;

        AddDomainEvent(new ProcessDefinitionRevisedEvent(
            EventId: Uuid7.New(),
            ProcessDefinitionId: ProcessDefinitionId.Value,
            RevisedByUserId: revisedBy.Value,
            Version: Version,
            RevisedAtUtc: revisedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -------------------------------------------------------------

    private void AddDomainEvent(IProcessDefinitionDomainEvent evt) => _domainEvents.Add(evt);

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ProcessDefinitionStatus, bool> guard, ProcessDefinitionStatus expected)
    {
        if (guard(Status)) return;

        throw new ProcessDefinitionInvalidStatusTransitionException(
            ProcessDefinitionId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
