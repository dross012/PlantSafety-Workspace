// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Infrastructure
//  File:           RegulatoryCalendarReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RegulatoryCalendar Application-layer read-model.
//                  Backs GetRegulatoryCalendarByIdQuery and GetRegulatoryCalendarQuery. Reuses
//                  usp_RegulatoryCalendar_GetById (the same proc
//                  RegulatoryCalendarAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate) and usp_RegulatoryCalendar_GetRange rather than
//                  introducing duplicate stored procedures. Mirrors
//                  PermitsReadDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Persistence;

/// <summary>Dapper-backed repository for RegulatoryCalendar read-model queries.</summary>
public sealed class RegulatoryCalendarReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RegulatoryCalendarReadDapperRepository> logger)
    : IRegulatoryCalendarRepository
{
    /// <inheritdoc/>
    public async Task<RegulatoryCalendarItemDto?> GetByIdAsync(Guid regulatoryCalendarId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for RegulatoryCalendar {RegulatoryCalendarId} Tenant {TenantId}", regulatoryCalendarId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RegulatoryCalendarId", regulatoryCalendarId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<RegulatoryCalendarItemDto>(
            "[Compliance].[usp_RegulatoryCalendar_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RegulatoryCalendarItemDto>> GetRegulatoryCalendarAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for RegulatoryCalendar tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RegulatoryCalendarItemDto>(
            "[Compliance].[usp_RegulatoryCalendar_GetRange]", parameters, cancellationToken);
    }
}
