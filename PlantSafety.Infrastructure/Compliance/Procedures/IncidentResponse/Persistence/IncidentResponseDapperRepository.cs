// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresIncidentResponse
//  Layer:          Infrastructure
//  File:           IncidentResponseDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresIncidentResponse read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.IncidentResponse.Models;
using PlantSafety.Application.Compliance.Procedures.IncidentResponse.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.IncidentResponse.Persistence;

/// <summary>Dapper-backed repository for <see cref="IncidentResponseItemDto"/> queries.</summary>
public sealed class IncidentResponseDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentResponseDapperRepository> logger)
    : IIncidentResponseRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentResponseItemDto>> GetIncidentResponseProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIncidentResponseProceduresAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IncidentResponseItemDto>(
            "[Compliance].[usp_ProceduresIncidentResponse_List]", parameters, cancellationToken);
    }
}