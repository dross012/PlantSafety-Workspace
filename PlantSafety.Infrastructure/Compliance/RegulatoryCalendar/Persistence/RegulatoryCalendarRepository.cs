// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Infrastructure
//  File:           RegulatoryCalendarRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RegulatoryCalendar aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Persistence;

/// <summary>Dapper-backed repository for <see cref="RegulatoryCalendarReadModel"/> queries.</summary>
public sealed class RegulatoryCalendarRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<RegulatoryCalendarRepository> logger)
{
    /// <summary>Returns the RegulatoryCalendar read model or <c>null</c> if not found.</summary>
    public async Task<RegulatoryCalendarReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for RegulatoryCalendar {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("RegulatoryCalendarId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<RegulatoryCalendarReadModel>(
            $"[Compliance].[usp_RegulatoryCalendar_GetById]", parameters);
    }

    /// <summary>Returns all RegulatoryCalendar records for the tenant.</summary>
    public async Task<IReadOnlyList<RegulatoryCalendarReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for RegulatoryCalendar tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<RegulatoryCalendarReadModel>(
            $"[Compliance].[usp_RegulatoryCalendar_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
