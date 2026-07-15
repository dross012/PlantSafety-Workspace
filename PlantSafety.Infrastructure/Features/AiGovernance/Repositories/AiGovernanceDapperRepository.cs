// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.AiGovernance
//  Layer:          Infrastructure
//  File:           AiGovernanceDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the governed AI/ML model read/write model. All DB access
//                  via stored procedures in the [AiGovernance] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.AiGovernance.Handlers;

namespace PlantSafety.Infrastructure.Features.AiGovernance.Repositories;

/// <summary>
/// Dapper-backed repository for governed AI/ML model read/write operations.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AiGovernanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<AiGovernanceDapperRepository> logger)
    : IAiGovernanceRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid modelId,
        Guid tenantId,
        Guid? siteId,
        string modelName,
        string modelVersion,
        string riskTier,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Register governed model {ModelId} for tenant {TenantId}", modelId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("ModelId", modelId)
            .AddGuid("SiteId", siteId)
            .AddString("ModelName", modelName, 200)
            .AddString("ModelVersion", modelVersion, 50)
            .AddString("RiskTier", riskTier, 20)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[AiGovernance].[usp_Model_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<GovernedModelReadModel?> GetByIdAsync(Guid tenantId, Guid modelId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get governed model {ModelId} for tenant {TenantId}", modelId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ModelId", modelId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<GovernedModelReadModel>(
            "[AiGovernance].[usp_Model_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GovernedModelReadModel>> ListAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        logger.LogDebug("List governed models for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<GovernedModelReadModel>(
            "[AiGovernance].[usp_Model_List]", parameters, cancellationToken);
    }
}
