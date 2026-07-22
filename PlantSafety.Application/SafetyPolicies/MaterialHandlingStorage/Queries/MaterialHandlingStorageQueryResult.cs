// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetMaterialHandlingStorageQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Models;

namespace PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Queries;

/// <summary>Result of <see cref="GetMaterialHandlingStorageQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Material Handling & Storage Policies items.</param>
public sealed record MaterialHandlingStorageQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MaterialHandlingStorageItemDto> Items);