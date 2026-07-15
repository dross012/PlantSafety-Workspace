// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.InspectionsDOT
//  Layer:          Infrastructure
//  File:           InspectionsDOTDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the InspectionsDOT read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.DOT.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.DOT.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Inspections.DOT.Persistence;

/// <summary>Dapper-backed repository for <see cref="DOTItemDto"/> queries.</summary>
public sealed class InspectionsDOTDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<InspectionsDOTDapperRepository> logger)
    : IDOTRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DOTItemDto>> GetInspectionsAsync(
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

        return await queryExecutor.QueryAsync<DOTItemDto>(
            "[Compliance].[usp_InspectionsDOT_List]", parameters, cancellationToken);
    }
}