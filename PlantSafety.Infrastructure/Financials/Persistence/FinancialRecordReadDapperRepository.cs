// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Infrastructure
//  File:           FinancialRecordReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the FinancialRecord Application-layer read-model. Backs
//                  FinancialRecordByIdQuery. Reuses usp_FinancialRecord_GetById and
//                  usp_FinancialRecordLineItem_GetByFinancialRecordId (the same procs
//                  FinancialRecordAggregateDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence
//                  .AuditingReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Financials.Models;
using PlantSafety.Application.Financials.Repositories;

namespace PlantSafety.Infrastructure.Financials.Persistence;

/// <summary>Dapper-backed repository for FinancialRecord read-model queries.</summary>
public sealed class FinancialRecordReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<FinancialRecordReadDapperRepository> logger)
    : IFinancialRecordDetailRepository
{
    /// <inheritdoc/>
    public async Task<FinancialRecordDetailDto?> GetByIdAsync(Guid financialRecordId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for FinancialRecord {FinancialRecordId} Tenant {TenantId}", financialRecordId, tenantId);

        var headerParameters = new StoredProcedureBuilder()
            .AddGuid("FinancialRecordId", financialRecordId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<FinancialRecordHeaderRow>(
            "[Financials].[usp_FinancialRecord_GetById]", headerParameters, cancellationToken);

        if (row is null) return null;

        var lineItemParameters = new StoredProcedureBuilder()
            .AddGuid("FinancialRecordId", financialRecordId)
            .Build();

        var lineItemRows = await queryExecutor.QueryAsync<FinancialRecordLineItemRow>(
            "[Financials].[usp_FinancialRecordLineItem_GetByFinancialRecordId]", lineItemParameters, cancellationToken);

        return new FinancialRecordDetailDto
        {
            FinancialRecordId = row.FinancialRecordId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            Status = row.Status,
            Category = row.Category,
            Amount = row.Amount,
            LineItems = lineItemRows
                .Select(li => new FinancialRecordLineItemDto
                {
                    LineItemId = li.LineItemId,
                    Description = li.Description,
                    Amount = li.Amount,
                })
                .ToList(),
            RecordedAtUtc = row.RecordedAtUtc,
            RecordedBy = row.RecordedBy,
            ETag = row.ETag,
        };
    }

    /// <summary>Flat row shape returned by <c>usp_FinancialRecord_GetById</c>.</summary>
    private sealed record FinancialRecordHeaderRow
    {
        public string? ETag { get; init; }

        public required Guid FinancialRecordId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string Category { get; init; }
        public required decimal Amount { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
    }

    /// <summary>Row shape returned by <c>usp_FinancialRecordLineItem_GetByFinancialRecordId</c>.</summary>
    private sealed record FinancialRecordLineItemRow
    {
        public required Guid LineItemId { get; init; }
        public required Guid FinancialRecordId { get; init; }
        public required string Description { get; init; }
        public required decimal Amount { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
    }
}
