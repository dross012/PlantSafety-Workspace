// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpiration.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PermitExpiration bounded context (module: PermitToWork).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates;

/// <summary>Aggregate root for the PermitExpiration bounded context.</summary>
public sealed partial class PermitExpiration
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PermitExpiration aggregate.</summary>
    public PermitExpirationId PermitExpirationId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitExpirationStatus Status { get; private set; }

    // ---- PermitExpiration-specific business properties --------------------------------

    /// <summary>PermitNumber business attribute.</summary>
    public string PermitNumber { get; private set; }

    /// <summary>IssuedAtUtc business attribute.</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>ExpiresAtUtc business attribute.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>ExtensionRequested business attribute.</summary>
    public bool ExtensionRequested { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitExpirationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitExpirationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitExpirationDomainEvent>(_domainEvents);

    // For rehydration only.
    private PermitExpiration() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PermitExpiration aggregate and raises <see cref="PermitExpirationCreatedEvent"/>.</summary>
    public static PermitExpiration Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitNumber = "",
        UtcTimestamp? issuedAtUtc = null,
        UtcTimestamp? expiresAtUtc = null,
        bool extensionRequested = false)
    {
        var agg = new PermitExpiration
        {
            PermitExpirationId     = PermitExpirationId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PermitExpirationStatus.Created,
            PermitNumber = (permitNumber ?? string.Empty).Trim(),
            IssuedAtUtc = issuedAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            ExtensionRequested = extensionRequested,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PermitExpirationCreatedEvent(
            EventId:       new PermitExpirationEventId(Uuid7.New()),
            PermitExpirationId:      agg.PermitExpirationId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitExpirationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitExpirationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PermitExpiration {PermitExpirationId}.");
    }
}