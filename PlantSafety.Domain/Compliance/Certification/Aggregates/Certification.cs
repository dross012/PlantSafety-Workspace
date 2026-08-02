// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           Certification.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  personnel-certification record shape (holder, certifying body,
//                  issue/expiry, renewal) grounded in project_plantsafety_domains.md
//                  Module 3 (Certification: Personnel certification lifecycle
//                  management). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Certification bounded context. Tracks a
//                  single personnel certification record: holder, issuing body,
//                  validity window, and renewal state.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Aggregates;

/// <summary>Aggregate root for the Certification bounded context.</summary>
public sealed partial class Certification
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Certification aggregate.</summary>
    public CertificationId CertificationId { get; private set; }

    /// <summary>Tenant that owns this certification record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this certification is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CertificationStatus Status { get; private set; }

    // ---- Certification-specific business properties ------------------------

    /// <summary>Worker who holds this certification.</summary>
    public UserId HolderWorkerId { get; private set; }

    /// <summary>Name of the certification (e.g., Confined Space Entry Supervisor).</summary>
    public string CertificationName { get; private set; } = string.Empty;

    /// <summary>Issuing/certifying body (e.g., OSHA, NEBOSH).</summary>
    public string CertificationBody { get; private set; } = string.Empty;

    /// <summary>Certificate/credential number as issued by the certifying body.</summary>
    public string CertificationNumber { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the certification was issued.</summary>
    public UtcTimestamp IssuedAtUtc { get; private set; }

    /// <summary>UTC timestamp the certification expires, if it has a validity window.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>Indicates whether this certification requires periodic renewal.</summary>
    public bool RenewalRequired { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICertificationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICertificationDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICertificationDomainEvent>(_domainEvents);

    // For rehydration only.
    private Certification() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Certification aggregate and raises <see cref="CertificationCreatedEvent"/>.</summary>
    public static Certification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        UserId holderWorkerId = default,
        string certificationName = "",
        string certificationBody = "",
        string certificationNumber = "",
        UtcTimestamp? issuedAtUtc = null,
        UtcTimestamp? expiresAtUtc = null,
        bool renewalRequired = true)
    {
        var agg = new Certification
        {
            CertificationId      = CertificationId.New(),
            TenantId             = tenantId,
            SiteId               = siteId,
            Status               = CertificationStatus.Created,
            HolderWorkerId       = holderWorkerId,
            CertificationName    = (certificationName ?? string.Empty).Trim(),
            CertificationBody    = (certificationBody ?? string.Empty).Trim(),
            CertificationNumber  = (certificationNumber ?? string.Empty).Trim(),
            IssuedAtUtc          = issuedAtUtc ?? createdAtUtc,
            ExpiresAtUtc         = expiresAtUtc,
            RenewalRequired      = renewalRequired,
            CreatedAtUtc         = createdAtUtc,
            CreatedBy            = createdBy,
        };

        agg.AddDomainEvent(new CertificationCreatedEvent(
            EventId:       new CertificationEventId(Uuid7.New()),
            CertificationId:      agg.CertificationId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Verify / Renew / Expire / Close / Archive ---------------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory. Mirrors Compliance.Permits' lifecycle build-out shape.

    /// <summary>
    /// Verifies and activates a certification record (Created -&gt; Active). Valid only from
    /// <see cref="CertificationStatus.Created"/>.
    /// </summary>
    public void Verify(
        UserId verifiedBy,
        UtcTimestamp verifiedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CertificationLifecycleRules.CanActivate, CertificationStatus.Created);

        Status = CertificationStatus.Active;

        AddDomainEvent(new CertificationVerifiedEvent(
            EventId: new CertificationEventId(Uuid7.New()),
            CertificationId: CertificationId,
            VerifiedBy: verifiedBy,
            VerifiedAtUtc: verifiedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Renews an active certification, extending its expiry and clearing the renewal-required
    /// flag. Does not change lifecycle status. Valid only from <see cref="CertificationStatus.Active"/>.
    /// </summary>
    public void Renew(
        UserId renewedBy,
        UtcTimestamp newExpiresAtUtc,
        UtcTimestamp renewedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CertificationLifecycleRules.CanRenew, CertificationStatus.Active);

        ExpiresAtUtc = newExpiresAtUtc;
        RenewalRequired = false;

        AddDomainEvent(new CertificationRenewedEvent(
            EventId: new CertificationEventId(Uuid7.New()),
            CertificationId: CertificationId,
            RenewedBy: renewedBy,
            NewExpiresAtUtc: newExpiresAtUtc,
            RenewedAtUtc: renewedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Expires an active certification that has passed its validity window (Active -&gt; Completed).
    /// Valid only from <see cref="CertificationStatus.Active"/>.
    /// </summary>
    public void Expire(
        UtcTimestamp expiredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CertificationLifecycleRules.CanComplete, CertificationStatus.Active);

        Status = CertificationStatus.Completed;

        AddDomainEvent(new CertificationExpiredEvent(
            EventId: new CertificationEventId(Uuid7.New()),
            CertificationId: CertificationId,
            ExpiredAtUtc: expiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a completed/expired certification (Completed -&gt; Closed). Valid only from
    /// <see cref="CertificationStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CertificationLifecycleRules.CanClose, CertificationStatus.Completed);

        Status = CertificationStatus.Closed;

        AddDomainEvent(new CertificationClosedEvent(
            EventId: new CertificationEventId(Uuid7.New()),
            CertificationId: CertificationId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed certification for compliance retention (Closed -&gt; Archived). Valid only
    /// from <see cref="CertificationStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CertificationLifecycleRules.CanArchive, CertificationStatus.Closed);

        Status = CertificationStatus.Archived;

        AddDomainEvent(new CertificationArchivedEvent(
            EventId: new CertificationEventId(Uuid7.New()),
            CertificationId: CertificationId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICertificationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params CertificationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Certification {CertificationId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<CertificationStatus, bool> guard, CertificationStatus expected)
    {
        if (guard(Status)) return;

        throw new CertificationInvalidStatusTransitionException(
            CertificationId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
