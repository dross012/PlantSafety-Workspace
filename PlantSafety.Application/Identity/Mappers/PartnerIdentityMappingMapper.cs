// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Deterministic mapper from the PartnerIdentityMapping domain
//   aggregate to its Application-layer DTO.
// SchemaVersion: 1

using PlantSafety.Application.Identity.DTOs;
using PlantSafety.Domain.Identity.Aggregates;

namespace PlantSafety.Application.Identity.Mappers;

/// <summary>Maps <see cref="PartnerIdentityMapping"/> aggregates to <see cref="PartnerIdentityMappingDto"/>.</summary>
public static class PartnerIdentityMappingMapper
{
    /// <summary>Maps a <see cref="PartnerIdentityMapping"/> aggregate to its DTO representation.</summary>
    /// <param name="aggregate">The aggregate to map.</param>
    public static PartnerIdentityMappingDto ToDto(PartnerIdentityMapping aggregate) => new()
    {
        PartnerIdentityMappingId = aggregate.PartnerIdentityMappingId.Value,
        TenantId = aggregate.TenantId,
        PartnerIssuer = aggregate.PartnerIssuer,
        PartnerExternalId = aggregate.PartnerExternalId,
        InternalUserId = aggregate.InternalUserId.Value,
        RegisteredAtUtc = aggregate.RegisteredAtUtc.Value,
        IsActive = aggregate.IsActive,
    };
}
