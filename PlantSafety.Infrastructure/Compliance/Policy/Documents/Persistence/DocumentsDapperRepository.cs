// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyDocuments
//  Layer:          Infrastructure
//  File:           DocumentsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyDocuments read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Documents.Models;
using PlantSafety.Application.Compliance.Policy.Documents.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.Documents.Persistence;

/// <summary>Dapper-backed repository for <see cref="DocumentsItemDto"/> queries.</summary>
public sealed class DocumentsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<DocumentsDapperRepository> logger)
    : IDocumentsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DocumentsItemDto>> GetDocumentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDocumentsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<DocumentsItemDto>(
            "[Compliance].[usp_PolicyDocuments_List]", parameters, cancellationToken);
    }
}