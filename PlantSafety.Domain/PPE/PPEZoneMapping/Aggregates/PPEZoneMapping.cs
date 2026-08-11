// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMapping.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PPEZoneMapping bounded context (module: PPE).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates;

/// <summary>Aggregate root for the PPEZoneMapping bounded context.</summary>
public sealed partial class PPEZoneMapping
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPEZoneMapping aggregate.</summary>
    public PPEZoneMappingId PPEZoneMappingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPEZoneMappingStatus Status { get; private set; }

    // ---- PPEZoneMapping-specific business properties --------------------------------

    /// <summary>ZoneCode business attribute.</summary>
    public string ZoneCode { get; private set; }

    /// <summary>RequiredPpeCategories business attribute.</summary>
    public string RequiredPpeCategories { get; private set; }

    /// <summary>AccessControlIntegrated business attribute.</summary>
    public bool AccessControlIntegrated { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPEZoneMappingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPEZoneMappingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPEZoneMappingDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPEZoneMapping() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        RequiredPpeCategories = null!;
        ZoneCode = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPEZoneMapping aggregate and raises <see cref="PPEZoneMappingCreatedEvent"/>.</summary>
    public static PPEZoneMapping Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string zoneCode = "",
        string requiredPpeCategories = "",
        bool accessControlIntegrated = false)
    {
        var agg = new PPEZoneMapping
        {
            PPEZoneMappingId     = PPEZoneMappingId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PPEZoneMappingStatus.Created,
            ZoneCode = (zoneCode ?? string.Empty).Trim(),
            RequiredPpeCategories = (requiredPpeCategories ?? string.Empty).Trim(),
            AccessControlIntegrated = accessControlIntegrated,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PPEZoneMappingCreatedEvent(
            EventId:       new PPEZoneMappingEventId(Uuid7.New()),
            PPEZoneMappingId:      agg.PPEZoneMappingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPEZoneMappingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPEZoneMappingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPEZoneMapping {PPEZoneMappingId}.");
    }
}