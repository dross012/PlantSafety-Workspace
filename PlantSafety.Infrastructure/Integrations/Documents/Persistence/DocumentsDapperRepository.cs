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

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<DocumentsItemDto> Items, int TotalCount)> ListDocumentsAsync(
        Guid tenantId,
        Guid? siteId,
        string? documentType,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ListDocuments for Integrations.Documents tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("DocumentType", documentType, 100)
            .AddString("Search", search, 200)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        // Mirrors the two-call pagination pattern used by Alerts.Alert.GetAlertHistoryAsync
        // (AlertQueryDapperRepository.QueryHistoryAsync): a dedicated count procedure plus a
        // dedicated paged-rows procedure, both driven from the same filter parameter set, rather
        // than a single QueryMultiple call.
        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int>(
            "[Integrations].[usp_Documents_ListDocumentsCount]", parameters, cancellationToken);
        var items = await queryExecutor.QueryAsync<DocumentsItemDto>(
            "[Integrations].[usp_Documents_ListDocumentsPage]", parameters, cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<DocumentsItemDto?> GetDocumentByIdAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDocumentById for Integrations.Documents tenant {TenantId}, document {DocumentId}", tenantId, documentId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("DocumentId", documentId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DocumentsItemDto>(
            "[Integrations].[usp_Documents_GetDocumentById]", parameters, cancellationToken);
    }
}
