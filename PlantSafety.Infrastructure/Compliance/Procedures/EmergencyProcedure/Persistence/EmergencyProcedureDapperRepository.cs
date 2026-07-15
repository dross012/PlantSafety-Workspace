// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresEmergencyProcedure
//  Layer:          Infrastructure
//  File:           EmergencyProcedureDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresEmergencyProcedure read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Models;
using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.EmergencyProcedure.Persistence;

/// <summary>Dapper-backed repository for <see cref="EmergencyProcedureItemDto"/> queries.</summary>
public sealed class EmergencyProcedureDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EmergencyProcedureDapperRepository> logger)
    : IEmergencyProcedureRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmergencyProcedureItemDto>> GetProceduresAsync(
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

        return await queryExecutor.QueryAsync<EmergencyProcedureItemDto>(
            "[Compliance].[usp_ProceduresEmergencyProcedure_List]", parameters, cancellationToken);
    }
}