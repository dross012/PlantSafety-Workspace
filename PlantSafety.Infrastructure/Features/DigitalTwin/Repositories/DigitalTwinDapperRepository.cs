// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.DigitalTwin
//  Layer:          Infrastructure
//  File:           DigitalTwinDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DigitalTwin asset read/write model. All DB access via
//                  stored procedures in the [DigitalTwin] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.DigitalTwin.Handlers;

namespace PlantSafety.Infrastructure.Features.DigitalTwin.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>DigitalTwin</c> asset read/write model.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class DigitalTwinDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<DigitalTwinDapperRepository> logger)
    : IDigitalTwinRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid twinId,
        Guid tenantId,
        Guid? siteId,
        string assetName,
        string assetType,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Register DigitalTwin {TwinId} for tenant {TenantId}", twinId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("TwinId", twinId)
            .AddGuid("SiteId", siteId)
            .AddString("AssetName", assetName, 200)
            .AddString("AssetType", assetType, 50)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[DigitalTwin].[usp_Twin_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<DigitalTwinReadModel?> GetByIdAsync(Guid tenantId, Guid twinId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get DigitalTwin {TwinId} for tenant {TenantId}", twinId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("TwinId", twinId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DigitalTwinReadModel>(
            "[DigitalTwin].[usp_Twin_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DigitalTwinReadModel>> ListAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        logger.LogDebug("List DigitalTwins for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<DigitalTwinReadModel>(
            "[DigitalTwin].[usp_Twin_List]", parameters, cancellationToken);
    }
}
