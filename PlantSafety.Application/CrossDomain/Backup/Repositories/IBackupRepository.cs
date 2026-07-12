// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Backup read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Backup.Models;

namespace PlantSafety.Application.CrossDomain.Backup.Repositories;

/// <summary>Read-only repository for Backup data. Implemented in the Infrastructure layer.</summary>
public interface IBackupRepository
{
    /// <summary>Returns Backup items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<BackupItemDto>> GetBackupJobsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
