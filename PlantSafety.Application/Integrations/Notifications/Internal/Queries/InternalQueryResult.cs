// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetInternalQuery.
// SchemaVersion: 1
using PlantSafety.Application.Integrations.Notifications.Internal.Models;

namespace PlantSafety.Application.Integrations.Notifications.Internal.Queries;

/// <summary>Result of <see cref="GetInternalQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Internal items.</param>
public sealed record InternalQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<InternalItemDto> Items);