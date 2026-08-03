// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Application-layer DTO for a partner identity mapping
//   (Phase 6 §2.2 "Tenant Federation" — Partner identity mapping).
// SchemaVersion: 1
// Additive-only evolution: new nullable properties may be appended.

namespace PlantSafety.Application.Identity.DTOs;

/// <summary>Read-model DTO describing a single partner identity mapping.</summary>
public sealed record PartnerIdentityMappingDto
{
    /// <summary>The mapping's unique identifier.</summary>
    public Guid PartnerIdentityMappingId { get; init; }

    /// <summary>The tenant this mapping is scoped to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>The partner's token issuer.</summary>
    public string PartnerIssuer { get; init; } = string.Empty;

    /// <summary>The partner's own external identity value.</summary>
    public string PartnerExternalId { get; init; } = string.Empty;

    /// <summary>The internal PlantSafety user this partner identity resolves to.</summary>
    public Guid InternalUserId { get; init; }

    /// <summary>UTC timestamp this mapping was registered.</summary>
    public DateTime RegisteredAtUtc { get; init; }

    /// <summary><see langword="true"/> when this mapping has not been revoked.</summary>
    public bool IsActive { get; init; }
}
