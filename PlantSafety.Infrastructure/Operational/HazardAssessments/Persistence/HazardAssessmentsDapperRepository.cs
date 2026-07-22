// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.HazardAssessments
//  Layer:          Infrastructure
//  File:           HazardAssessmentsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazardAssessments read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.HazardAssessments.Models;
using PlantSafety.Application.Operational.HazardAssessments.Repositories;

namespace PlantSafety.Infrastructure.Operational.HazardAssessments.Persistence;

/// <summary>Dapper-backed repository for <see cref="HazardAssessmentsItemDto"/> queries.</summary>
public sealed class HazardAssessmentsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<HazardAssessmentsDapperRepository> logger)
    : IHazardAssessmentsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazardAssessmentsItemDto>> GetHazardAssessmentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardAssessmentsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HazardAssessmentsItemDto>(
            "[Operational].[usp_HazardAssessments_List]", parameters, cancellationToken);
    }
}
