// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWork.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single Permit-to-Work (PTW) authorizing a high-risk contractor
//                  task: hot work, confined space entry, electrical/isolation, working at height, or
//                  excavation. Models the real lifecycle: draft, hazard assessment (incl. atmospheric
//                  testing for confined space entry), issuance with a validity window, suspension for
//                  unsafe conditions, closure, and expiry.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates;

/// <summary>Aggregate root for a single Permit-to-Work (PTW) authorizing a high-risk contractor task.</summary>
public sealed partial class PermitToWork
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PermitToWork aggregate.</summary>
    public PermitToWorkId PermitToWorkId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitToWorkStatus Status { get; private set; }

    // ---- Permit identity ---------------------------------------------------------

    /// <summary>Human-readable title of this permit.</summary>
    public string PermitTitle { get; private set; } = string.Empty;

    /// <summary>Permit type.</summary>
    public PermitType PermitType { get; private set; }

    /// <summary>Scope/location of the authorized work.</summary>
    public string WorkLocation { get; private set; } = string.Empty;

    /// <summary>Number of hazards identified in the hazard assessment.</summary>
    public int HazardCount { get; private set; }

    /// <summary>Whether atmospheric testing is required (confined space entry permits).</summary>
    public bool RequiresAtmosphericTesting { get; private set; }

    /// <summary>Atmospheric test score/result (0-100), when applicable and recorded.</summary>
    public decimal? AtmosphericTestScore { get; private set; }

    /// <summary>Identity of the employee who authorizes/issues the permit.</summary>
    public UserId AuthorizedByUserId { get; private set; }

    /// <summary>UTC timestamp the permit was issued, if any.</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>UTC timestamp the permit expires.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>Narrative reason recorded when the permit is suspended.</summary>
    public string? SuspensionReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitToWorkDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitToWorkDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitToWorkDomainEvent>(_domainEvents);

    // For rehydration only.
    private PermitToWork() { }

    // ---- Factory: Create (Draft) -----------------------------------------------------------

    /// <summary>Drafts a new permit-to-work request and raises <see cref="PermitToWorkDraftedEvent"/>.</summary>
    public static PermitToWork Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitTitle = "",
        PermitType permitType = PermitType.HotWork,
        string workLocation = "")
    {
        var agg = new PermitToWork
        {
            PermitToWorkId              = PermitToWorkId.New(),
            TenantId                    = tenantId,
            SiteId                      = siteId,
            Status                      = PermitToWorkStatus.Draft,
            PermitTitle                 = (permitTitle ?? string.Empty).Trim(),
            PermitType                  = permitType,
            WorkLocation                = (workLocation ?? string.Empty).Trim(),
            RequiresAtmosphericTesting  = permitType == PermitType.ConfinedSpaceEntry,
            CreatedAtUtc                = createdAtUtc,
            CreatedBy                   = createdBy,
        };

        agg.AddDomainEvent(new PermitToWorkDraftedEvent(
            PermitToWorkEventId.New(), agg.PermitToWorkId, permitType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordHazardAssessment -----------------------------------------------

    /// <summary>Records the hazard assessment, including atmospheric testing results for confined space entry.</summary>
    public void RecordHazardAssessment(int hazardCount, decimal? atmosphericTestScore, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PermitToWorkStatus.Draft);

        if (hazardCount < 0)
            throw new ArgumentOutOfRangeException(nameof(hazardCount), "Hazard count cannot be negative.");
        if (RequiresAtmosphericTesting && atmosphericTestScore is null)
            throw new PermitToWorkInvalidStatusTransitionException(PermitToWorkId, Status.ToString(), PermitToWorkStatus.HazardAssessed.ToString(), correlationId, causationId);

        HazardCount = hazardCount;
        AtmosphericTestScore = atmosphericTestScore;
        Status = PermitToWorkStatus.HazardAssessed;

        AddDomainEvent(new PermitToWorkHazardAssessmentRecordedEvent(
            PermitToWorkEventId.New(), PermitToWorkId, hazardCount, atmosphericTestScore, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Issue -----------------------------------------------------------

    /// <summary>Issues the permit, authorizing work for a bounded validity window.</summary>
    public void Issue(UserId authorizedByUserId, UtcTimestamp issuedAtUtc, int validityHours, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PermitToWorkStatus.HazardAssessed);

        if (validityHours <= 0)
            throw new ArgumentOutOfRangeException(nameof(validityHours), "Validity hours must be positive.");

        AuthorizedByUserId = authorizedByUserId;
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = new UtcTimestamp(issuedAtUtc.Value.AddHours(validityHours));
        Status = PermitToWorkStatus.Issued;

        AddDomainEvent(new PermitToWorkIssuedEvent(
            PermitToWorkEventId.New(), PermitToWorkId, authorizedByUserId, ExpiresAtUtc.Value, issuedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Suspend -----------------------------------------------------------

    /// <summary>Suspends an active permit because an unsafe condition was discovered.</summary>
    public void Suspend(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PermitToWorkStatus.Issued);

        SuspensionReason = reason;
        Status = PermitToWorkStatus.Suspended;

        AddDomainEvent(new PermitToWorkSuspendedEvent(
            PermitToWorkEventId.New(), PermitToWorkId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes out the permit once work is complete.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PermitToWorkStatus.Issued, PermitToWorkStatus.Suspended);

        Status = PermitToWorkStatus.Closed;

        AddDomainEvent(new PermitToWorkClosedEvent(
            PermitToWorkEventId.New(), PermitToWorkId, closedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Expire -----------------------------------------------------------

    /// <summary>Expires the permit once its validity window elapses without closure.</summary>
    public void Expire(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PermitToWorkStatus.Issued);

        Status = PermitToWorkStatus.Expired;

        AddDomainEvent(new PermitToWorkExpiredEvent(
            PermitToWorkEventId.New(), PermitToWorkId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitToWorkDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitToWorkStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PermitToWork {PermitToWorkId}.");
    }
}
