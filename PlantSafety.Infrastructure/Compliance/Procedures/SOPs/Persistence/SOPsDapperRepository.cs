// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresSOPs
//  Layer:          Infrastructure
//  File:           SOPsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresSOPs read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.SOPs.Models;
using PlantSafety.Application.Compliance.Procedures.SOPs.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.SOPs.Persistence;

/// <summary>Dapper-backed repository for <see cref="SOPsItemDto"/> queries.</summary>
public sealed class SOPsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SOPsDapperRepository> logger)
    : ISOPsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SOPsItemDto>> GetSOPsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSOPsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SOPsItemDto>(
            "[Compliance].[usp_ProceduresSOPs_List]", parameters, cancellationToken);
    }
}