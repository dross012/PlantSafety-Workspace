// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Backup
//  Layer:          Infrastructure
//  File:           BackupDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Backup read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Backup.Models;
using PlantSafety.Application.CrossDomain.Backup.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Backup.Persistence;

/// <summary>Dapper-backed repository for <see cref="BackupItemDto"/> queries.</summary>
public sealed class BackupDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<BackupDapperRepository> logger)
    : IBackupRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BackupItemDto>> GetBackupJobsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetBackupJobsAsync for Backup tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<BackupItemDto>(
            "[CrossDomain].[usp_Backup_GetBackupJobs]", parameters, cancellationToken);
    }
}
