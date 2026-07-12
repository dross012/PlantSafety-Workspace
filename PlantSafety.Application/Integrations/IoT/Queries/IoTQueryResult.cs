// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetIoTQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.IoT.Models;

namespace PlantSafety.Application.Integrations.IoT.Queries;

/// <summary>Result of <see cref="GetIoTQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.IoT items.</param>
public sealed record IoTQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IoTItemDto> Items);
