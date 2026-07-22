// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientation.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single contractor site-specific orientation record. Trains
//                  contractors on the facility's unique hazards: plant layout, restricted zones,
//                  emergency signals/evacuation, chemical hazards/SDS access, machine guarding rules,
//                  and PPE requirements. Models the real lifecycle: scheduling, delivery, comprehension
//                  verification, and expiry.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates;

/// <summary>Aggregate root for a single contractor site-specific orientation record.</summary>
public sealed partial class SiteOrientation
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this SiteOrientation aggregate.</summary>
    public SiteOrientationId SiteOrientationId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SiteOrientationStatus Status { get; private set; }

    // ---- Orientation identity ---------------------------------------------------

    /// <summary>Human-readable title of this orientation session.</summary>
    public string OrientationTitle { get; private set; } = string.Empty;

    /// <summary>Contractor company name being oriented.</summary>
    public string ContractorName { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who conducted the orientation.</summary>
    public UserId ConductedByUserId { get; private set; }

    /// <summary>UTC timestamp the orientation was delivered, if recorded.</summary>
    public UtcTimestamp? DeliveredAtUtc { get; private set; }

    /// <summary>Number of contractor personnel who attended.</summary>
    public int AttendeeCount { get; private set; }

    /// <summary>Comprehension score (0-100) from the post-orientation check.</summary>
    public decimal? ComprehensionScore { get; private set; }

    /// <summary>Minimum passing comprehension score.</summary>
    public decimal PassingScoreThreshold { get; private set; } = 80m;

    /// <summary>UTC timestamp this orientation expires and re-orientation is required.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ISiteOrientationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISiteOrientationDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISiteOrientationDomainEvent>(_domainEvents);

    // For rehydration only.
    private SiteOrientation() { }

    // ---- Factory: Create -----------------------------------------------------------

    /// <summary>Schedules a new site orientation session and raises <see cref="SiteOrientationScheduledEvent"/>.</summary>
    public static SiteOrientation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string orientationTitle = "",
        string contractorName = "",
        UserId conductedByUserId = default,
        decimal passingScoreThreshold = 80m)
    {
        var agg = new SiteOrientation
        {
            SiteOrientationId      = SiteOrientationId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = SiteOrientationStatus.Scheduled,
            OrientationTitle       = (orientationTitle ?? string.Empty).Trim(),
            ContractorName         = (contractorName ?? string.Empty).Trim(),
            ConductedByUserId      = conductedByUserId,
            PassingScoreThreshold  = passingScoreThreshold,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new SiteOrientationScheduledEvent(
            SiteOrientationEventId.New(), agg.SiteOrientationId, agg.ContractorName,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: DeliverOrientation -----------------------------------------------

    /// <summary>Records delivery of the orientation session.</summary>
    public void DeliverOrientation(int attendeeCount, UtcTimestamp deliveredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SiteOrientationStatus.Scheduled, SiteOrientationStatus.Expired);

        if (attendeeCount < 0)
            throw new ArgumentOutOfRangeException(nameof(attendeeCount), "Attendee count cannot be negative.");

        AttendeeCount  = attendeeCount;
        DeliveredAtUtc = deliveredAtUtc;
        Status = SiteOrientationStatus.Delivered;

        AddDomainEvent(new SiteOrientationDeliveredEvent(
            SiteOrientationEventId.New(), SiteOrientationId, attendeeCount, deliveredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: VerifyComprehension -----------------------------------------------

    /// <summary>Verifies comprehension and, if passed, clears the contractor for site access with an expiry date.</summary>
    public void VerifyComprehension(decimal comprehensionScore, UtcTimestamp verifiedAtUtc, int validityDays, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SiteOrientationStatus.Delivered);

        if (comprehensionScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(comprehensionScore), "Comprehension score must be between 0 and 100.");
        if (comprehensionScore < PassingScoreThreshold)
            throw new SiteOrientationInvalidStatusTransitionException(SiteOrientationId, Status.ToString(), SiteOrientationStatus.ComprehensionVerified.ToString(), correlationId, causationId);

        ComprehensionScore = comprehensionScore;
        ExpiresAtUtc = new UtcTimestamp(verifiedAtUtc.Value.AddDays(validityDays));
        Status = SiteOrientationStatus.ComprehensionVerified;

        AddDomainEvent(new SiteOrientationComprehensionVerifiedEvent(
            SiteOrientationEventId.New(), SiteOrientationId, comprehensionScore, ExpiresAtUtc.Value, verifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Expire -----------------------------------------------------------

    /// <summary>Expires the orientation once its validity period elapses.</summary>
    public void Expire(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SiteOrientationStatus.ComprehensionVerified);

        Status = SiteOrientationStatus.Expired;

        AddDomainEvent(new SiteOrientationExpiredEvent(
            SiteOrientationEventId.New(), SiteOrientationId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISiteOrientationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SiteOrientationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for SiteOrientation {SiteOrientationId}.");
    }
}
