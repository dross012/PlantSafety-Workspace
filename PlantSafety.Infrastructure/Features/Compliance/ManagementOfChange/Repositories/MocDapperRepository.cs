// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.Compliance.ManagementOfChange
//  Layer:          Infrastructure
//  File:           MocDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ManagementOfChange aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Features.Compliance.Handlers.ManagementOfChange;

namespace PlantSafety.Infrastructure.Features.Compliance.ManagementOfChange.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>ManagementOfChange</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class MocDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<MocDapperRepository> logger)
    : IMocRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid mocId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string changeType,
        string riskLevel,
        Guid initiatorId,
        DateTimeOffset proposedStartUtc,
        DateTimeOffset? proposedEndUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create MOC {MocId} for tenant {TenantId}", mocId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("MocId", mocId)
            .AddGuid("SiteId", siteId)
            .AddString("Title", title, 200)
            .AddString("Description", description)
            .AddString("ChangeType", changeType, 50)
            .AddString("RiskLevel", riskLevel, 50)
            .AddGuid("InitiatorId", initiatorId)
            .AddDateTime("ProposedStartUtc", proposedStartUtc.UtcDateTime)
            .AddDateTime("ProposedEndUtc", proposedEndUtc?.UtcDateTime)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_Moc_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task ApproveAsync(
        Guid tenantId,
        Guid mocId,
        string approvalNotes,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Approve MOC {MocId} for tenant {TenantId}", mocId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("MocId", mocId)
            .AddString("ApprovalNotes", approvalNotes)
            .AddString("ETag", eTag, 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_Moc_Approve]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"MOC {mocId} was modified by another process or the ETag is stale.", correlationId);
        }

        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<MocReadModel?> GetByIdAsync(Guid tenantId, Guid mocId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get MOC {MocId} for tenant {TenantId}", mocId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("MocId", mocId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<MocReadModel>(
            "[Compliance].[usp_Moc_GetById]", parameters, cancellationToken);
    }
}
