// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to resolve a partner's external identity to its
//   internal user mapping (Phase 6 §2.2 "Tenant Federation" — Partner identity
//   mapping). Read-only; no side effects.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Identity.DTOs;

namespace PlantSafety.Application.Identity.Queries;

/// <summary>
/// Resolves the active partner identity mapping for a given (tenant, issuer, partner
/// external id) tuple, or <see langword="null"/> when no active mapping exists.
/// </summary>
public sealed record ResolvePartnerIdentityMappingQuery(
    string TraceId,
    string SpanId,
    string ApiVersionToken,
    Guid RequestingUserId,
    string RequestingUserRole,
    Guid TenantId,
    Guid CorrelationId,
    CachePreference CachePreference,
    bool SuppressUnauthorized,
    bool StrictMode,
    string PartnerIssuer,
    string PartnerExternalId) : IQuery<PartnerIdentityMappingDto?>;
