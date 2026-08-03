// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Infrastructure
//  File:           PartInventoryReadDapperRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer
//                  IPartInventoryReadRepository (parts inventory list). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.WorkOrders.Models;
using PlantSafety.Application.Operational.WorkOrders.Repositories;

namespace PlantSafety.Infrastructure.Operational.WorkOrders.Persistence;

/// <summary>Dapper-backed repository for the Part inventory list query.</summary>
public sealed class PartInventoryReadDapperRepository(DapperQueryExecutor queryExecutor, ILogger<PartInventoryReadDapperRepository> logger)
    : IPartInventoryReadRepository
{
    /// <inheritdoc/>
    public async Task<(IReadOnlyList<PartInventoryDto> Items, int TotalCount)> ListAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogDebug("List Parts inventory for Tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<PartInventoryRow>("[Operational].[usp_Part_List]", parameters, cancellationToken);

        var items = rows.Select(r => new PartInventoryDto
        {
            PartId = r.PartId,
            PartNumber = r.PartNumber,
            PartName = r.PartName,
            QuantityOnHand = r.QuantityOnHand,
            ReorderThreshold = r.ReorderThreshold,
            Location = r.Location,
            NeedsReorder = r.NeedsReorder,
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    private sealed record PartInventoryRow
    {
        public required Guid PartId { get; init; }
        public required string PartNumber { get; init; }
        public required string PartName { get; init; }
        public required int QuantityOnHand { get; init; }
        public required int ReorderThreshold { get; init; }
        public required string Location { get; init; }
        public required bool NeedsReorder { get; init; }
        public required int TotalCount { get; init; }
    }
}
