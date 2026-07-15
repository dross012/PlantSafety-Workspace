// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyRevisions
//  Layer:          Infrastructure
//  File:           RevisionsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyRevisions read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Revisions.Models;
using PlantSafety.Application.Compliance.Policy.Revisions.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.Revisions.Persistence;

/// <summary>Dapper-backed repository for <see cref="RevisionsItemDto"/> queries.</summary>
public sealed class RevisionsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<RevisionsDapperRepository> logger)
    : IRevisionsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RevisionsItemDto>> GetRevisionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRevisionsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RevisionsItemDto>(
            "[Compliance].[usp_PolicyRevisions_List]", parameters, cancellationToken);
    }
}