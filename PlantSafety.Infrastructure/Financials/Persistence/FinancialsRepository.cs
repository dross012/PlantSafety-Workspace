// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Infrastructure
//  File:           FinancialsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for Financials entities.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Financials.Persistence;

/// <summary>Dapper-backed repository for <see cref="FinancialsReadModel"/> queries.</summary>
public sealed class FinancialsRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<FinancialsRepository> logger)
{
    /// <summary>Returns the financial record or <c>null</c> if not found.</summary>
    public async Task<FinancialsReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById Financial {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("FinancialId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<FinancialsReadModel>(
            conn, "[Financials].[usp_Financial_GetById]", parameters);
    }
}
