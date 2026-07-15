// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresSafety
//  Layer:          Infrastructure
//  File:           SafetyDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresSafety read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.Safety.Models;
using PlantSafety.Application.Compliance.Procedures.Safety.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.Safety.Persistence;

/// <summary>Dapper-backed repository for <see cref="SafetyItemDto"/> queries.</summary>
public sealed class SafetyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SafetyDapperRepository> logger)
    : ISafetyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyItemDto>> GetSafetyProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSafetyProceduresAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SafetyItemDto>(
            "[Compliance].[usp_ProceduresSafety_List]", parameters, cancellationToken);
    }
}