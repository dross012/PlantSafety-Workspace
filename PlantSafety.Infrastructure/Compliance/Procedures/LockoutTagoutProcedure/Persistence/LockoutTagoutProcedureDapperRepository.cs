// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresLockoutTagoutProcedure
//  Layer:          Infrastructure
//  File:           LockoutTagoutProcedureDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresLockoutTagoutProcedure read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Models;
using PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.LockoutTagoutProcedure.Persistence;

/// <summary>Dapper-backed repository for <see cref="LockoutTagoutProcedureItemDto"/> queries.</summary>
public sealed class LockoutTagoutProcedureDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<LockoutTagoutProcedureDapperRepository> logger)
    : ILockoutTagoutProcedureRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<LockoutTagoutProcedureItemDto>> GetProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetProceduresAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LockoutTagoutProcedureItemDto>(
            "[Compliance].[usp_ProceduresLockoutTagoutProcedure_List]", parameters, cancellationToken);
    }
}