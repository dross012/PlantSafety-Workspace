// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetBackupQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Backup.Models;

namespace PlantSafety.Application.CrossDomain.Backup.Queries;

/// <summary>Result of <see cref="GetBackupQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Backup items.</param>
public sealed record BackupQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<BackupItemDto> Items);
