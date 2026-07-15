// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresWorkInstructions
//  Layer:          Infrastructure
//  File:           WorkInstructionsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresWorkInstructions read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.WorkInstructions.Models;
using PlantSafety.Application.Compliance.Procedures.WorkInstructions.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.WorkInstructions.Persistence;

/// <summary>Dapper-backed repository for <see cref="WorkInstructionsItemDto"/> queries.</summary>
public sealed class WorkInstructionsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<WorkInstructionsDapperRepository> logger)
    : IWorkInstructionsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkInstructionsItemDto>> GetWorkInstructionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWorkInstructionsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WorkInstructionsItemDto>(
            "[Compliance].[usp_ProceduresWorkInstructions_List]", parameters, cancellationToken);
    }
}