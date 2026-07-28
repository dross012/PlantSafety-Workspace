// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.Financials.ROI
//  Layer:          Infrastructure
//  File:           RoiDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyROI read/write model. All DB access via stored
//                  procedures in the [Financials] schema. Additive-only evolution.
//  UPDATED:        2026-07-28 -- maps the ETag column now projected by usp_Roi_GetByFiscalYear/
//                  usp_Roi_List onto RoiReadModel.ETag. Schema-only: no conflict-rejection logic
//                  is introduced here (see RoiReadModel.ETag remarks).
// ============================================================================
using PlantSafety.Application.Features.Financials.Handlers.ROI;

namespace PlantSafety.Infrastructure.Features.Financials.ROI.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>SafetyROI</c> read/write model.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class RoiDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<RoiDapperRepository> logger)
    : IRoiRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid roiCalculationId,
        Guid tenantId,
        Guid? siteId,
        int fiscalYear,
        decimal totalInvestment,
        decimal estimatedAvoidedCosts,
        decimal netBenefit,
        decimal roiPercentage,
        string currencyCode,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create ROI {RoiCalculationId} for tenant {TenantId}", roiCalculationId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("RoiCalculationId", roiCalculationId)
            .AddGuid("SiteId", siteId)
            .AddInt("FiscalYear", fiscalYear)
            .AddDecimal("TotalInvestment", totalInvestment)
            .AddDecimal("EstimatedAvoidedCosts", estimatedAvoidedCosts)
            .AddDecimal("NetBenefit", netBenefit)
            .AddDecimal("RoiPercentage", roiPercentage)
            .AddString("CurrencyCode", currencyCode, 3)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Financials].[usp_Roi_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<RoiReadModel?> GetByFiscalYearAsync(Guid tenantId, Guid? siteId, int fiscalYear, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get ROI for tenant {TenantId} FiscalYear {FiscalYear}", tenantId, fiscalYear);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddInt("FiscalYear", fiscalYear)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RoiRow>(
            "[Financials].[usp_Roi_GetByFiscalYear]", parameters, cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new RoiReadModel
        {
            RoiCalculationId = row.RoiCalculationId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            FiscalYear = row.FiscalYear,
            TotalInvestment = row.TotalInvestment,
            EstimatedAvoidedCosts = row.EstimatedAvoidedCosts,
            NetBenefit = row.NetBenefit,
            RoiPercentage = row.RoiPercentage,
            CurrencyCode = row.CurrencyCode,
            CalculatedAtUtc = row.CalculatedAtUtc,
            ETag = row.ETag,
        };
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RoiReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List ROI history for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<RoiListRow>(
            "[Financials].[usp_Roi_List]", parameters, cancellationToken);

        var items = rows.Select(r => new RoiReadModel
        {
            RoiCalculationId = r.RoiCalculationId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            FiscalYear = r.FiscalYear,
            TotalInvestment = r.TotalInvestment,
            EstimatedAvoidedCosts = r.EstimatedAvoidedCosts,
            NetBenefit = r.NetBenefit,
            RoiPercentage = r.RoiPercentage,
            CurrencyCode = r.CurrencyCode,
            CalculatedAtUtc = r.CalculatedAtUtc,
            ETag = r.ETag,
        }).ToList();

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;
        return (items, totalCount);
    }

    /// <summary>Raw row shape returned by <c>[Financials].[usp_Roi_GetByFiscalYear]</c>.</summary>
    private sealed record RoiRow
    {
        public required Guid RoiCalculationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required int FiscalYear { get; init; }
        public required decimal TotalInvestment { get; init; }
        public required decimal EstimatedAvoidedCosts { get; init; }
        public required decimal NetBenefit { get; init; }
        public required decimal RoiPercentage { get; init; }
        public required string CurrencyCode { get; init; }
        public required DateTimeOffset CalculatedAtUtc { get; init; }

        /// <summary>See <see cref="IRoiRepository"/>'s <c>RoiReadModel.ETag</c> remarks -- not yet consumed by any write path.</summary>
        public string? ETag { get; init; }
    }

    /// <summary>Raw row shape returned by <c>[Financials].[usp_Roi_List]</c>, including the windowed total row count.</summary>
    private sealed record RoiListRow
    {
        public required Guid RoiCalculationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required int FiscalYear { get; init; }
        public required decimal TotalInvestment { get; init; }
        public required decimal EstimatedAvoidedCosts { get; init; }
        public required decimal NetBenefit { get; init; }
        public required decimal RoiPercentage { get; init; }
        public required string CurrencyCode { get; init; }
        public required DateTimeOffset CalculatedAtUtc { get; init; }

        /// <summary>See <see cref="IRoiRepository"/>'s <c>RoiReadModel.ETag</c> remarks -- not yet consumed by any write path.</summary>
        public string? ETag { get; init; }
        public required int TotalCount { get; init; }
    }
}
