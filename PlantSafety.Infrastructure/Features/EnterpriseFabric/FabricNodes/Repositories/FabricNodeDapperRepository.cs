// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.EnterpriseFabric.FabricNodes
//  Layer:          Infrastructure
//  File:           FabricNodeDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the integration fabric node read/write model. All DB
//                  access via stored procedures in the [EnterpriseFabric] schema.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.EnterpriseFabric.Handlers.FabricNodes;

namespace PlantSafety.Infrastructure.Features.EnterpriseFabric.FabricNodes.Repositories;

/// <summary>
/// Dapper-backed repository for integration fabric node read/write operations.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class FabricNodeDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<FabricNodeDapperRepository> logger)
    : IFabricNodeRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid nodeId,
        Guid tenantId,
        Guid? siteId,
        string nodeName,
        string nodeType,
        string region,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Register fabric node {NodeId} for tenant {TenantId}", nodeId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("NodeId", nodeId)
            .AddGuid("SiteId", siteId)
            .AddString("NodeName", nodeName, 200)
            .AddString("NodeType", nodeType, 50)
            .AddString("Region", region, 50)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[EnterpriseFabric].[usp_FabricNode_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<FabricNodeReadModel?> GetByIdAsync(Guid tenantId, Guid nodeId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get fabric node {NodeId} for tenant {TenantId}", nodeId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("NodeId", nodeId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<FabricNodeReadModel>(
            "[EnterpriseFabric].[usp_FabricNode_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FabricNodeReadModel>> ListAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        logger.LogDebug("List fabric nodes for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<FabricNodeReadModel>(
            "[EnterpriseFabric].[usp_FabricNode_List]", parameters, cancellationToken);
    }
}
