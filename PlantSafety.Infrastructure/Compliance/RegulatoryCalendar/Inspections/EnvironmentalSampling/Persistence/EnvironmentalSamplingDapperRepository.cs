// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.InspectionsEnvironmentalSampling
//  Layer:          Infrastructure
//  File:           EnvironmentalSamplingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the InspectionsEnvironmentalSampling read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Persistence;

/// <summary>Dapper-backed repository for <see cref="EnvironmentalSamplingItemDto"/> queries.</summary>
public sealed class EnvironmentalSamplingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EnvironmentalSamplingDapperRepository> logger)
    : IEnvironmentalSamplingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EnvironmentalSamplingItemDto>> GetInspectionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetInspectionsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EnvironmentalSamplingItemDto>(
            "[Compliance].[usp_InspectionsEnvironmentalSampling_List]", parameters, cancellationToken);
    }
}