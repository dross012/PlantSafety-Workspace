// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Identity
//  Layer:          Domain
//  File:           PartnerIdentityMapping.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  SchemaVersion:  2026.1
//  Description:    Aggregate root binding a federated partner's external identity
//                  (issuer + external id) to an internal PlantSafety user, scoped
//                  to a tenant (Phase 6 §2.2 "Tenant Federation" — Partner identity
//                  mapping). Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.Domain.Identity.Events;
using PlantSafety.Domain.Identity.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Identity.Aggregates;

/// <summary>
/// Aggregate root for a single partner identity mapping: binds a federated partner's
/// external identity (its token issuer plus its own external identity value) to an
/// internal PlantSafety user, scoped to a tenant.
/// </summary>
public sealed class PartnerIdentityMapping
{
    /// <summary>Unique identity of this mapping.</summary>
    public PartnerIdentityMappingId PartnerIdentityMappingId { get; private set; }

    /// <summary>Tenant that owns this mapping.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>The partner's token issuer ("iss" claim), e.g. the partner's own IdP.</summary>
    public string PartnerIssuer { get; private set; } = string.Empty;

    /// <summary>The partner's own external identity value (subject/user identifier at the partner IdP).</summary>
    public string PartnerExternalId { get; private set; } = string.Empty;

    /// <summary>The internal PlantSafety user this partner identity resolves to.</summary>
    public UserId InternalUserId { get; private set; }

    /// <summary>UTC timestamp this mapping was registered.</summary>
    public UtcTimestamp RegisteredAtUtc { get; private set; }

    /// <summary>Identity of the user/admin who registered this mapping.</summary>
    public UserId RegisteredBy { get; private set; }

    /// <summary>UTC timestamp this mapping was revoked, or <see langword="null"/> when still active.</summary>
    public UtcTimestamp? RevokedAtUtc { get; private set; }

    /// <summary><see langword="true"/> when this mapping has not been revoked.</summary>
    public bool IsActive => RevokedAtUtc is null;

    private readonly List<IPartnerIdentityMappingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPartnerIdentityMappingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPartnerIdentityMappingDomainEvent>(_domainEvents);

    // For rehydration only.
    private PartnerIdentityMapping() { }

    /// <summary>
    /// Registers a new partner identity mapping and raises
    /// <see cref="PartnerIdentityMappingRegisteredEvent"/>.
    /// </summary>
    /// <param name="registeredBy">CRUD authorship identity of the admin registering this mapping.</param>
    /// <param name="registeredAtUtc">Registration timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant. Required.</param>
    /// <param name="partnerIssuer">The partner's token issuer. Required.</param>
    /// <param name="partnerExternalId">The partner's own external identity value. Required.</param>
    /// <param name="internalUserId">The internal PlantSafety user this maps to. Required.</param>
    public static PartnerIdentityMapping Register(
        UserId registeredBy,
        UtcTimestamp registeredAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        string partnerIssuer,
        string partnerExternalId,
        UserId internalUserId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(partnerIssuer))
            throw new ArgumentException("PartnerIssuer is required.", nameof(partnerIssuer));
        if (string.IsNullOrWhiteSpace(partnerExternalId))
            throw new ArgumentException("PartnerExternalId is required.", nameof(partnerExternalId));

        var agg = new PartnerIdentityMapping
        {
            PartnerIdentityMappingId = PartnerIdentityMappingId.New(),
            TenantId = tenantId,
            PartnerIssuer = partnerIssuer.Trim(),
            PartnerExternalId = partnerExternalId.Trim(),
            InternalUserId = internalUserId,
            RegisteredAtUtc = registeredAtUtc,
            RegisteredBy = registeredBy,
        };

        agg._domainEvents.Add(new PartnerIdentityMappingRegisteredEvent(
            EventId: Uuid7.New(),
            PartnerIdentityMappingId: agg.PartnerIdentityMappingId.Value,
            TenantId: tenantId,
            PartnerIssuer: agg.PartnerIssuer,
            PartnerExternalId: agg.PartnerExternalId,
            InternalUserId: internalUserId.Value,
            RegisteredAtUtc: registeredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    /// <summary>Revokes this mapping. Idempotent — revoking an already-revoked mapping is a no-op.</summary>
    /// <param name="revokedAtUtc">Revocation timestamp (UTC).</param>
    public void Revoke(UtcTimestamp revokedAtUtc)
    {
        RevokedAtUtc ??= revokedAtUtc;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
