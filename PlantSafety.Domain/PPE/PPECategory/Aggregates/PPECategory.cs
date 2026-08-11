// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PPECategory bounded context (module: PPE).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Aggregates;

/// <summary>Aggregate root for the PPECategory bounded context.</summary>
public sealed partial class PPECategory
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPECategory aggregate.</summary>
    public PPECategoryId PPECategoryId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPECategoryStatus Status { get; private set; }

    // ---- PPECategory-specific business properties --------------------------------

    /// <summary>CategoryCode business attribute.</summary>
    public string CategoryCode { get; private set; }

    /// <summary>HazardsMitigated business attribute.</summary>
    public string HazardsMitigated { get; private set; }

    /// <summary>InspectionIntervalDays business attribute.</summary>
    public int InspectionIntervalDays { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPECategoryDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPECategoryDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPECategoryDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPECategory() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        CategoryCode = null!;
        HazardsMitigated = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPECategory aggregate and raises <see cref="PPECategoryCreatedEvent"/>.</summary>
    public static PPECategory Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string categoryCode = "",
        string hazardsMitigated = "",
        int inspectionIntervalDays = 0)
    {
        var agg = new PPECategory
        {
            PPECategoryId     = PPECategoryId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PPECategoryStatus.Created,
            CategoryCode = (categoryCode ?? string.Empty).Trim(),
            HazardsMitigated = (hazardsMitigated ?? string.Empty).Trim(),
            InspectionIntervalDays = inspectionIntervalDays,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PPECategoryCreatedEvent(
            EventId:       new PPECategoryEventId(Uuid7.New()),
            PPECategoryId:      agg.PPECategoryId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPECategoryDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPECategoryStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPECategory {PPECategoryId}.");
    }
}