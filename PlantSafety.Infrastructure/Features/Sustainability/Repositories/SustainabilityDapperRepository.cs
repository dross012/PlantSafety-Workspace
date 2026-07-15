// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.Sustainability
//  Layer:          Infrastructure
//  File:           SustainabilityDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the sustainability metric read/write model. All DB access
//                  via stored procedures in the [Sustainability] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.Sustainability.Handlers;

namespace PlantSafety.Infrastructure.Features.Sustainability.Repositories;

/// <summary>
/// Dapper-backed repository for sustainability metric read/write operations.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class SustainabilityDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<SustainabilityDapperRepository> logger)
    : ISustainabilityRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid metricId,
        Guid tenantId,
        Guid? siteId,
        string metricName,
        string category,
        double value,
        string unit,
        double? target,
        DateTime periodEndUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Record sustainability metric {MetricId} for tenant {TenantId}", metricId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("MetricId", metricId)
            .AddGuid("SiteId", siteId)
            .AddString("MetricName", metricName, 200)
            .AddString("Category", category, 50)
            .AddDecimal("Value", (decimal)value)
            .AddString("Unit", unit, 20)
            .AddDecimal("Target", target.HasValue ? (decimal?)target.Value : null)
            .AddDateTime("PeriodEndUtc", periodEndUtc)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Sustainability].[usp_Metric_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<SustainabilityMetricReadModel?> GetByIdAsync(Guid tenantId, Guid metricId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get sustainability metric {MetricId} for tenant {TenantId}", metricId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("MetricId", metricId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<SustainabilityMetricReadModel>(
            "[Sustainability].[usp_Metric_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SustainabilityMetricReadModel>> ListAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("List sustainability metrics for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SustainabilityMetricReadModel>(
            "[Sustainability].[usp_Metric_List]", parameters, cancellationToken);
    }
}
