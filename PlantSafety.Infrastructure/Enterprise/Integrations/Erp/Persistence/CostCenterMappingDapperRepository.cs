// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Infrastructure
//  File:           CostCenterMappingDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ICostCenterMappingRepository, backed by the real
//                  [Enterprise].[CostCenterMappings] table. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Infrastructure.Enterprise.Integrations.Erp.Persistence;

/// <summary>Dapper-backed repository for <see cref="CostCenterMapping"/> reference data.</summary>
public sealed class CostCenterMappingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ILogger<CostCenterMappingDapperRepository> logger)
    : ICostCenterMappingRepository
{
    /// <inheritdoc/>
    public async Task<CostCenterMapping?> GetAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAsync for CostCenterMapping Tenant {TenantId} Site {SiteId}", tenantId, siteId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CostCenterMappingRow>(
            "[Enterprise].[usp_CostCenterMappings_Get]", parameters, cancellationToken);

        return row is null
            ? null
            : new CostCenterMapping(
                row.TenantId,
                row.SiteId,
                row.ExternalCostCenterCode,
                row.Description,
                UtcTimestamp.From(row.UpdatedAtUtc),
                new UserId(row.UpdatedBy));
    }

    /// <inheritdoc/>
    public async Task UpsertAsync(CostCenterMapping mapping, CancellationToken cancellationToken)
    {
        logger.LogDebug("UpsertAsync for CostCenterMapping Tenant {TenantId} Site {SiteId}", mapping.TenantId, mapping.SiteId);

        using var conn = connectionFactory.CreateConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", mapping.TenantId)
            .AddGuid("SiteId", mapping.SiteId)
            .AddString("ExternalCostCenterCode", mapping.ExternalCostCenterCode, 50)
            .AddString("Description", mapping.Description, 200)
            .AddDateTime("UpdatedAtUtc", mapping.UpdatedAtUtc.Value)
            .AddGuid("UpdatedBy", mapping.UpdatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(
            conn, "[Enterprise].[usp_CostCenterMappings_Upsert]", parameters, cancellationToken: cancellationToken);
    }

    /// <summary>Flat read-model row shape returned by <c>usp_CostCenterMappings_Get</c>.</summary>
    private sealed record CostCenterMappingRow
    {
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ExternalCostCenterCode { get; init; }
        public string? Description { get; init; }
        public required DateTime UpdatedAtUtc { get; init; }
        public required Guid UpdatedBy { get; init; }
    }
}
