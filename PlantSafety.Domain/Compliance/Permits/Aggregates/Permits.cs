// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           Permits.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  Permit-to-Work record shape grounded in project_plantsafety_domains.md
//                  Module 12 (Permits: "Permit-to-Work lifecycle management", flagged
//                  as a Critical Module). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Permits bounded context. Tracks a single
//                  permit-to-work: its type, work description, location, hazards, and
//                  issue/expiry window.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Aggregates;

/// <summary>Aggregate root for the Permits bounded context.</summary>
public sealed partial class Permits
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Permits aggregate.</summary>
    public PermitsId PermitsId { get; private set; }

    /// <summary>Tenant that owns this permit record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this permit is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitsStatus Status { get; private set; }

    // ---- Permit-specific business properties --------------------------------

    /// <summary>Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).</summary>
    public string PermitType { get; private set; } = string.Empty;

    /// <summary>Human-readable permit number/reference.</summary>
    public string PermitNumber { get; private set; } = string.Empty;

    /// <summary>Description of the work covered by this permit.</summary>
    public string WorkDescription { get; private set; } = string.Empty;

    /// <summary>Physical location the permit applies to.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Hazards identified for this permitted work.</summary>
    public string HazardsIdentified { get; private set; } = string.Empty;

    /// <summary>Identity of the user who requested the permit.</summary>
    public UserId RequestedByUserId { get; private set; }

    /// <summary>Identity of the user who approved/issued the permit, if issued.</summary>
    public UserId? ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the permit was issued.</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>UTC timestamp the permit expires.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitsDomainEvent>(_domainEvents);

    // For rehydration only.
    private Permits() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Permits aggregate and raises <see cref="PermitsCreatedEvent"/>.</summary>
    public static Permits Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitType = "",
        string permitNumber = "",
        string workDescription = "",
        string location = "",
        string hazardsIdentified = "",
        UserId requestedByUserId = default,
        UserId? approvedByUserId = null,
        UtcTimestamp? issuedAtUtc = null,
        UtcTimestamp? expiresAtUtc = null)
    {
        var agg = new Permits
        {
            PermitsId           = PermitsId.New(),
            TenantId             = tenantId,
            SiteId               = siteId,
            Status               = PermitsStatus.Created,
            PermitType           = (permitType ?? string.Empty).Trim(),
            PermitNumber         = (permitNumber ?? string.Empty).Trim(),
            WorkDescription      = (workDescription ?? string.Empty).Trim(),
            Location             = (location ?? string.Empty).Trim(),
            HazardsIdentified    = (hazardsIdentified ?? string.Empty).Trim(),
            RequestedByUserId    = requestedByUserId,
            ApprovedByUserId     = approvedByUserId,
            IssuedAtUtc          = issuedAtUtc,
            ExpiresAtUtc         = expiresAtUtc,
            CreatedAtUtc         = createdAtUtc,
            CreatedBy            = createdBy,
        };

        agg.AddDomainEvent(new PermitsCreatedEvent(
            EventId:       new PermitsEventId(Uuid7.New()),
            PermitsId:      agg.PermitsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Permits {PermitsId}.");
    }
}
