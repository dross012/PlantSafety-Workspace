// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           GlobalSafetyInsightsReadDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer
//                  IGlobalSafetyInsightsReadRepository, projecting the same [GlobalSafetyInsights].
//                  [Snapshot] rows the Domain repository reads into the flatter
//                  GlobalSafetyInsightsItemDto read-model shape for the dashboard/query handler.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Application.GlobalSafetyInsights.Repositories;
using PlantSafety.Infrastructure.Common.Serialization;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Persistence;

/// <summary>Dapper-backed read-only repository for the GlobalSafetyInsights read-model.</summary>
public sealed class GlobalSafetyInsightsReadDapperRepository(
    DapperQueryExecutor queryExecutor, ILogger<GlobalSafetyInsightsReadDapperRepository> logger)
    : IGlobalSafetyInsightsReadRepository
{
    /// <inheritdoc/>
    public async Task<GlobalSafetyInsightsItemDto?> GetLatestAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLatest GlobalSafetyInsights for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<SnapshotReadRow>(
            "[GlobalSafetyInsights].[usp_GlobalSafetyInsights_GetLatest]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GlobalSafetyInsightsItemDto>> GetHistoryAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHistory GlobalSafetyInsights for Tenant {TenantId} [{From:u}..{To:u}]", tenantId, from, to);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        var rows = await queryExecutor.QueryAsync<SnapshotReadRow>(
            "[GlobalSafetyInsights].[usp_GlobalSafetyInsights_ListByTenant]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static GlobalSafetyInsightsItemDto ToDto(SnapshotReadRow row) => new()
    {
        GlobalSafetyInsightsId = row.GlobalSafetyInsightsId,
        TenantId = row.TenantId,
        RegionCode = row.RegionCode,
        SiteIds = JsonSerializer.Deserialize<List<Guid>>(row.SiteIdsJson, JsonSerializerConfiguration.Default) ?? [],
        WindowStartUtc = row.WindowStartUtc,
        WindowEndUtc = row.WindowEndUtc,
        Trir = row.Trir,
        Ltir = row.Ltir,
        ExposureRate = row.ExposureRate,
        IncidentDensity = row.IncidentDensity,
        MachineStateIncidentCorrelation = row.MachineStateIncidentCorrelation,
        AnomalyCount = (JsonSerializer.Deserialize<List<object>>(row.AnomaliesJson, JsonSerializerConfiguration.Default) ?? []).Count,
        RecommendationCount = (JsonSerializer.Deserialize<List<object>>(row.RecommendationsJson, JsonSerializerConfiguration.Default) ?? []).Count,
        ComputedAtUtc = row.ComputedAtUtc,
    };

    /// <summary>Flat read-model row shape returned by every usp_GlobalSafetyInsights_* query proc.</summary>
    private sealed record SnapshotReadRow
    {
        public required Guid GlobalSafetyInsightsId { get; init; }
        public required Guid TenantId { get; init; }
        public required string RegionCode { get; init; }
        public required string SiteIdsJson { get; init; }
        public required DateTime WindowStartUtc { get; init; }
        public required DateTime WindowEndUtc { get; init; }
        public required decimal Trir { get; init; }
        public required decimal Ltir { get; init; }
        public required decimal ExposureRate { get; init; }
        public required decimal IncidentDensity { get; init; }
        public required double MachineStateIncidentCorrelation { get; init; }
        public required string AnomaliesJson { get; init; }
        public required string RecommendationsJson { get; init; }
        public required DateTime ComputedAtUtc { get; init; }
    }
}
