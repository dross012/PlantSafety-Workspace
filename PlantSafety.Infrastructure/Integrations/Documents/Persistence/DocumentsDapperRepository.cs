// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Documents
//  Layer:          Infrastructure
//  File:           DocumentsDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Documents read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Documents.Models;
using PlantSafety.Application.Integrations.Documents.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Documents.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Documents read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class DocumentsDapperRepository(
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
        logger.LogDebug("GetDocuments for Integrations.Documents tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<DocumentsItemDto>(
            "[Integrations].[usp_Documents_GetDocuments]", parameters, cancellationToken);
    }
}
