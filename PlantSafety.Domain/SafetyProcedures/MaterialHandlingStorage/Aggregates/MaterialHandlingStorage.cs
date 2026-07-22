// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorage.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single material handling / storage procedure covering one
//                  handling category (lifting, forklift, pallet jack, crane, storage) and storage area.
//                  Models the real document/compliance lifecycle: drafting, approval, publication,
//                  obstruction flagging, resolution, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates;

/// <summary>Aggregate root for a single material handling / storage procedure.</summary>
public sealed partial class MaterialHandlingStorage
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this MaterialHandlingStorage aggregate.</summary>
    public MaterialHandlingStorageId MaterialHandlingStorageId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public MaterialHandlingStorageStatus Status { get; private set; }

    // ---- Procedure identity ---------------------------------------------------------

    /// <summary>Human-readable title of this procedure document.</summary>
    public string ProcedureTitle { get; private set; } = string.Empty;

    /// <summary>Material handling category this procedure governs.</summary>
    public MaterialHandlingCategory HandlingCategory { get; private set; }

    /// <summary>Storage/handling area this procedure applies to.</summary>
    public string StorageLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the approver.</summary>
    public UserId ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the document was published, if any.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next scheduled review is due.</summary>
    public UtcTimestamp? NextReviewDueAtUtc { get; private set; }

    /// <summary>Count of walkway/egress obstructions or unsafe storage conditions logged.</summary>
    public int ObstructionCount { get; private set; }

    /// <summary>Compliance score (0-100) from the most recent audit.</summary>
    public decimal? ComplianceScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IMaterialHandlingStorageDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IMaterialHandlingStorageDomainEvent> DomainEvents
        => new ReadOnlyCollection<IMaterialHandlingStorageDomainEvent>(_domainEvents);

    // For rehydration only.
    private MaterialHandlingStorage() { }

    // ---- Factory: Create (Draft) -----------------------------------------------------------

    /// <summary>Drafts a new material handling/storage procedure and raises <see cref="MaterialHandlingProcedureDraftedEvent"/>.</summary>
    public static MaterialHandlingStorage Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string procedureTitle = "",
        MaterialHandlingCategory handlingCategory = MaterialHandlingCategory.SafeLifting,
        string storageLocation = "")
    {
        var agg = new MaterialHandlingStorage
        {
            MaterialHandlingStorageId = MaterialHandlingStorageId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = MaterialHandlingStorageStatus.Draft,
            ProcedureTitle    = (procedureTitle ?? string.Empty).Trim(),
            HandlingCategory  = handlingCategory,
            StorageLocation   = (storageLocation ?? string.Empty).Trim(),
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg.AddDomainEvent(new MaterialHandlingProcedureDraftedEvent(
            MaterialHandlingStorageEventId.New(), agg.MaterialHandlingStorageId, handlingCategory, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: SubmitForApproval -----------------------------------------------

    /// <summary>Submits the draft for approval.</summary>
    public void SubmitForApproval(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MaterialHandlingStorageStatus.Draft);

        Status = MaterialHandlingStorageStatus.PendingApproval;

        AddDomainEvent(new MaterialHandlingProcedureSubmittedForApprovalEvent(
            MaterialHandlingStorageEventId.New(), MaterialHandlingStorageId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Publish -----------------------------------------------------------

    /// <summary>Approves and publishes the procedure.</summary>
    public void Publish(UserId approvedByUserId, UtcTimestamp publishedAtUtc, int reviewCycleDays, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MaterialHandlingStorageStatus.PendingApproval);

        ApprovedByUserId    = approvedByUserId;
        PublishedAtUtc      = publishedAtUtc;
        NextReviewDueAtUtc  = new UtcTimestamp(publishedAtUtc.Value.AddDays(reviewCycleDays));
        Status              = MaterialHandlingStorageStatus.Published;

        AddDomainEvent(new MaterialHandlingProcedurePublishedEvent(
            MaterialHandlingStorageEventId.New(), MaterialHandlingStorageId, approvedByUserId, publishedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: FlagObstruction -----------------------------------------------

    /// <summary>Flags a walkway/egress obstruction or unsafe storage condition found during a floor walkthrough.</summary>
    public void FlagObstruction(int obstructionCount, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MaterialHandlingStorageStatus.Published);

        if (obstructionCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(obstructionCount), "Obstruction count must be positive.");

        ObstructionCount = obstructionCount;
        Status = MaterialHandlingStorageStatus.ObstructionFlagged;

        AddDomainEvent(new MaterialHandlingObstructionFlaggedEvent(
            MaterialHandlingStorageEventId.New(), MaterialHandlingStorageId, obstructionCount, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ResolveObstruction -----------------------------------------------

    /// <summary>Resolves the flagged obstruction and returns the procedure to published/compliant state.</summary>
    public void ResolveObstruction(decimal complianceScore, UtcTimestamp resolvedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MaterialHandlingStorageStatus.ObstructionFlagged);

        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        ObstructionCount = 0;
        ComplianceScore  = complianceScore;
        Status           = MaterialHandlingStorageStatus.Published;

        AddDomainEvent(new MaterialHandlingObstructionResolvedEvent(
            MaterialHandlingStorageEventId.New(), MaterialHandlingStorageId, complianceScore, resolvedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the procedure document.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == MaterialHandlingStorageStatus.Retired)
            throw new MaterialHandlingStorageInvalidStatusTransitionException(MaterialHandlingStorageId, Status.ToString(), MaterialHandlingStorageStatus.Retired.ToString(), correlationId, causationId);

        Status = MaterialHandlingStorageStatus.Retired;

        AddDomainEvent(new MaterialHandlingProcedureRetiredEvent(
            MaterialHandlingStorageEventId.New(), MaterialHandlingStorageId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IMaterialHandlingStorageDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params MaterialHandlingStorageStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for MaterialHandlingStorage {MaterialHandlingStorageId}.");
    }
}
