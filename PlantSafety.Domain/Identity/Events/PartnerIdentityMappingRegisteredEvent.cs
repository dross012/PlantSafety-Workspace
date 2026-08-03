// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Identity
//  Layer:          Domain
//  File:           PartnerIdentityMappingRegisteredEvent.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a partner's external identity is
//                  registered/mapped to an internal user (Phase 6 §2.2 "Tenant
//                  Federation" — Partner identity mapping). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Identity.Events;

/// <summary>Raised when a new PartnerIdentityMapping aggregate is registered.</summary>
public sealed record PartnerIdentityMappingRegisteredEvent(
    Guid             EventId,
    Guid             PartnerIdentityMappingId,
    Guid             TenantId,
    string           PartnerIssuer,
    string           PartnerExternalId,
    Guid             InternalUserId,
    UtcTimestamp     RegisteredAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IPartnerIdentityMappingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PartnerIdentityMappingRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RegisteredAtUtc;
}
