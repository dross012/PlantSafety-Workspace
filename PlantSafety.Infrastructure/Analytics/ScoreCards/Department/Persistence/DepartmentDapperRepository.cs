// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards.Department
//  Layer:          Infrastructure
//  File:           DepartmentDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Department read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.ScoreCards.Department.Models;
using PlantSafety.Application.Analytics.ScoreCards.Department.Repositories;

namespace PlantSafety.Infrastructure.Analytics.ScoreCards.Department.Persistence;

/// <summary>Dapper-backed repository for <see cref="DepartmentItemDto"/> queries.</summary>
public sealed class DepartmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<DepartmentDapperRepository> logger)
    : IDepartmentRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DepartmentItemDto>> GetDepartmentScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDepartmentScoresAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<DepartmentItemDto>(
            "[Analytics].[usp_Department_List]", parameters, cancellationToken);
    }
}
