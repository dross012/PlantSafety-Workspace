// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresInspection
//  Layer:          Infrastructure
//  File:           InspectionDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresInspection read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.Inspection.Models;
using PlantSafety.Application.Compliance.Procedures.Inspection.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.Inspection.Persistence;

/// <summary>Dapper-backed repository for <see cref="InspectionItemDto"/> queries.</summary>
public sealed class InspectionDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<InspectionDapperRepository> logger)
    : IInspectionRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<InspectionItemDto>> GetInspectionProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetInspectionProceduresAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<InspectionItemDto>(
            "[Compliance].[usp_ProceduresInspection_List]", parameters, cancellationToken);
    }
}