// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.DeadlinesAnnualAudit
//  Layer:          Infrastructure
//  File:           AnnualAuditDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DeadlinesAnnualAudit read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Persistence;

/// <summary>Dapper-backed repository for <see cref="AnnualAuditItemDto"/> queries.</summary>
public sealed class AnnualAuditDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<AnnualAuditDapperRepository> logger)
    : IAnnualAuditRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AnnualAuditItemDto>> GetDeadlinesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDeadlinesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AnnualAuditItemDto>(
            "[Compliance].[usp_DeadlinesAnnualAudit_List]", parameters, cancellationToken);
    }
}