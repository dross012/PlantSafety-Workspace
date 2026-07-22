// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.CompetencyManagement
//  Layer:          Infrastructure
//  File:           CompetencyManagementDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CompetencyManagement read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.CompetencyManagement.Models;
using PlantSafety.Application.Operational.CompetencyManagement.Repositories;

namespace PlantSafety.Infrastructure.Operational.CompetencyManagement.Persistence;

/// <summary>Dapper-backed repository for <see cref="CompetencyManagementItemDto"/> queries.</summary>
public sealed class CompetencyManagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CompetencyManagementDapperRepository> logger)
    : ICompetencyManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CompetencyManagementItemDto>> GetCompetencyManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCompetencyManagementAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CompetencyManagementItemDto>(
            "[Operational].[usp_CompetencyManagement_List]", parameters, cancellationToken);
    }
}
