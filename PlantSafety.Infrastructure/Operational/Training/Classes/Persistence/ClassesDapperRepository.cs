// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Classes
//  Layer:          Infrastructure
//  File:           ClassesDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Classes read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Training.Classes.Models;
using PlantSafety.Application.Operational.Training.Classes.Repositories;

namespace PlantSafety.Infrastructure.Operational.Training.Classes.Persistence;

/// <summary>Dapper-backed repository for <see cref="ClassItemDto"/> queries.</summary>
public sealed class ClassesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ClassesDapperRepository> logger)
    : IClassesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ClassItemDto>> GetClassesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetClassesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ClassItemDto>(
            "[Operational].[usp_Classes_List]", parameters, cancellationToken);
    }
}
