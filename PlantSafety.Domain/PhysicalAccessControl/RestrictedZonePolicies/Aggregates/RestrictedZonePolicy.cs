// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root defining which credential access level(s) may enter a given
//                  SafetyZone, and whether after-hours access is locked out. Enforced by
//                  Anomaly Detection when correlating ingested AccessEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates;

/// <summary>Aggregate root for a single restricted-zone access policy.</summary>
public sealed partial class RestrictedZonePolicy
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this RestrictedZonePolicy aggregate.</summary>
    public RestrictedZonePolicyId RestrictedZonePolicyId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RestrictedZonePolicyStatus Status { get; private set; }

    // ---- Policy identity -----------------------------------------------------

    /// <summary>Identity of the SafetyZone this policy governs.</summary>
    public Guid SafetyZoneId { get; private set; }

    /// <summary>Human-readable name of this policy.</summary>
    public string PolicyName { get; private set; } = string.Empty;

    /// <summary>Minimum credential access-level label required to enter the zone.</summary>
    public string AllowedAccessLevel { get; private set; } = string.Empty;

    /// <summary>Whether after-hours access is locked out for this zone under this policy.</summary>
    public bool EnforceAfterHoursLockout { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IRestrictedZonePolicyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRestrictedZonePolicyDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRestrictedZonePolicyDomainEvent>(_domainEvents);

    // For rehydration only.
    private RestrictedZonePolicy() { }

    // ---- Factory: Create (Define) -----------------------------------------------------------

    /// <summary>Defines a new restricted-zone policy and raises <see cref="RestrictedZonePolicyDefinedEvent"/>.</summary>
    public static RestrictedZonePolicy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid safetyZoneId = default,
        string policyName = "",
        string allowedAccessLevel = "General",
        bool enforceAfterHoursLockout = false)
    {
        var agg = new RestrictedZonePolicy
        {
            RestrictedZonePolicyId   = RestrictedZonePolicyId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = RestrictedZonePolicyStatus.Draft,
            SafetyZoneId             = safetyZoneId,
            PolicyName               = (policyName ?? string.Empty).Trim(),
            AllowedAccessLevel       = (allowedAccessLevel ?? string.Empty).Trim(),
            EnforceAfterHoursLockout = enforceAfterHoursLockout,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new RestrictedZonePolicyDefinedEvent(
            RestrictedZonePolicyEventId.New(), agg.RestrictedZonePolicyId, safetyZoneId, agg.AllowedAccessLevel, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Activate -----------------------------------------------

    /// <summary>Activates the policy for enforcement.</summary>
    public void Activate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RestrictedZonePolicyStatus.Draft, RestrictedZonePolicyStatus.Suspended);

        Status = RestrictedZonePolicyStatus.Active;

        AddDomainEvent(new RestrictedZonePolicyActivatedEvent(
            RestrictedZonePolicyEventId.New(), RestrictedZonePolicyId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Suspend -----------------------------------------------------------

    /// <summary>Temporarily suspends enforcement of the policy.</summary>
    public void Suspend(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RestrictedZonePolicyStatus.Active);

        Status = RestrictedZonePolicyStatus.Suspended;

        AddDomainEvent(new RestrictedZonePolicySuspendedEvent(
            RestrictedZonePolicyEventId.New(), RestrictedZonePolicyId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Permanently retires the policy.</summary>
    public void Retire(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RestrictedZonePolicyStatus.Draft, RestrictedZonePolicyStatus.Active, RestrictedZonePolicyStatus.Suspended);

        Status = RestrictedZonePolicyStatus.Retired;

        AddDomainEvent(new RestrictedZonePolicyRetiredEvent(
            RestrictedZonePolicyEventId.New(), RestrictedZonePolicyId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRestrictedZonePolicyDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params RestrictedZonePolicyStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for RestrictedZonePolicy {RestrictedZonePolicyId}.");
    }
}
