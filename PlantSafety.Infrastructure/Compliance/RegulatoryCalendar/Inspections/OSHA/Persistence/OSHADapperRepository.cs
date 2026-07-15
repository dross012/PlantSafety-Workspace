// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.InspectionsOSHA
//  Layer:          Infrastructure
//  File:           OSHADapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the InspectionsOSHA read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.OSHA.Persistence;

/// <summary>Dapper-backed repository for <see cref="OSHAItemDto"/> queries.</summary>
public sealed class OSHADapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<OSHADapperRepository> logger)
    : IOSHARepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OSHAItemDto>> GetInspectionsAsync(
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

        return await queryExecutor.QueryAsync<OSHAItemDto>(
            "[Compliance].[usp_InspectionsOSHA_List]", parameters, cancellationToken);
    }
}