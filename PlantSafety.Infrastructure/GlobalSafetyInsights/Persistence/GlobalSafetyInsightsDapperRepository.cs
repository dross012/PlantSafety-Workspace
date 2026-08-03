// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           GlobalSafetyInsightsDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IGlobalSafetyInsightsRepository (Domain
//                  repository interface for the GlobalSafetyInsightsAggregate root). All DB access
//                  via stored procedures in the [GlobalSafetyInsights] schema, every call
//                  TenantId-scoped (zero cross-tenant leakage). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because GlobalSafetyInsightsAggregate exposes only a
//                  private "for rehydration only" constructor and private-setter properties, same
//                  convention as AccidentDapperRepository. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Entities;
using PlantSafety.Domain.GlobalSafetyInsights.Events;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.Specifications;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;
using PlantSafety.Infrastructure.Common.Serialization;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Persistence;

/// <summary>Dapper-backed repository for the <see cref="GlobalSafetyInsightsAggregate"/> root.</summary>
public sealed class GlobalSafetyInsightsDapperRepository(
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ISqlConnectionFactory connectionFactory,
    IOutboxWriter outboxWriter,
    ILogger<GlobalSafetyInsightsDapperRepository> logger)
    : IGlobalSafetyInsightsRepository
{
    /// <inheritdoc/>
    public async Task<GlobalSafetyInsightsAggregate?> GetByIdAsync(GlobalSafetyInsightsId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("GlobalSafetyInsightsId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SnapshotRow>(
            "[GlobalSafetyInsights].[usp_GlobalSafetyInsights_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<GlobalSafetyInsightsAggregate?> GetLatestAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SnapshotRow>(
            "[GlobalSafetyInsights].[usp_GlobalSafetyInsights_GetLatest]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GlobalSafetyInsightsAggregate>> ListAsync(
        Guid tenantId, GlobalSafetyInsightsSpecification specification, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();

        var rows = await queryExecutor.QueryAsync<SnapshotRow>(
            "[GlobalSafetyInsights].[usp_GlobalSafetyInsights_ListByTenant]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(GlobalSafetyInsightsAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add GlobalSafetyInsights snapshot {SnapshotId}", aggregate.GlobalSafetyInsightsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("GlobalSafetyInsightsId", aggregate.GlobalSafetyInsightsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("RegionCode", aggregate.Region.Code, 30)
            .AddString("SiteIdsJson", JsonSerializer.Serialize(aggregate.SiteIds, JsonSerializerConfiguration.Default))
            .AddDateTime("WindowStartUtc", aggregate.TimeWindow.StartUtc.Value)
            .AddDateTime("WindowEndUtc", aggregate.TimeWindow.EndUtc.Value)
            .AddDecimal("Trir", aggregate.Kpis.Trir)
            .AddDecimal("Ltir", aggregate.Kpis.Ltir)
            .AddDecimal("ExposureRate", aggregate.Kpis.ExposureRate)
            .AddDecimal("IncidentDensity", aggregate.Kpis.IncidentDensity)
            .AddDouble("MachineStateIncidentCorrelation", aggregate.Kpis.MachineStateIncidentCorrelation)
            .AddInt("TotalRecordableIncidents", aggregate.Kpis.TotalRecordableIncidents)
            .AddInt("TotalLostTimeIncidents", aggregate.Kpis.TotalLostTimeIncidents)
            .AddDecimal("TotalHoursWorked", aggregate.Kpis.TotalHoursWorked)
            .AddString("RiskScoresJson", JsonSerializer.Serialize(aggregate.RiskScores, JsonSerializerConfiguration.Default))
            .AddString("AnomaliesJson", JsonSerializer.Serialize(aggregate.Anomalies, JsonSerializerConfiguration.Default))
            .AddString("RecommendationsJson", JsonSerializer.Serialize(aggregate.Recommendations, JsonSerializerConfiguration.Default))
            .AddDateTime("ComputedAtUtc", aggregate.ComputedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[GlobalSafetyInsights].[usp_GlobalSafetyInsights_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="GlobalSafetyInsightsAggregate"/> from its flat read-model row.</summary>
    private static GlobalSafetyInsightsAggregate Hydrate(SnapshotRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<GlobalSafetyInsightsAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(GlobalSafetyInsightsAggregate.GlobalSafetyInsightsId), GlobalSafetyInsightsId.FromGuid(row.GlobalSafetyInsightsId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(GlobalSafetyInsightsAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(GlobalSafetyInsightsAggregate.Region), Region.From(row.RegionCode));
        AggregateReflectionHydrator.SetProperty(agg, nameof(GlobalSafetyInsightsAggregate.TimeWindow),
            new SafetyAnalysisTimeWindow(UtcTimestamp.From(row.WindowStartUtc), UtcTimestamp.From(row.WindowEndUtc)));
        AggregateReflectionHydrator.SetProperty(agg, nameof(GlobalSafetyInsightsAggregate.Kpis), new GlobalSafetyKpiSnapshot
        {
            Trir = row.Trir,
            Ltir = row.Ltir,
            ExposureRate = row.ExposureRate,
            IncidentDensity = row.IncidentDensity,
            MachineStateIncidentCorrelation = row.MachineStateIncidentCorrelation,
            TotalRecordableIncidents = row.TotalRecordableIncidents,
            TotalLostTimeIncidents = row.TotalLostTimeIncidents,
            TotalHoursWorked = row.TotalHoursWorked,
        });
        AggregateReflectionHydrator.SetProperty(agg, nameof(GlobalSafetyInsightsAggregate.ComputedAtUtc), UtcTimestamp.From(row.ComputedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(GlobalSafetyInsightsAggregate.CreatedBy), new UserId(row.CreatedBy));

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IGlobalSafetyInsightsDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_siteIds",
            JsonSerializer.Deserialize<List<Guid>>(row.SiteIdsJson, JsonSerializerConfiguration.Default) ?? []);
        AggregateReflectionHydrator.SetField(agg, "_riskScores",
            JsonSerializer.Deserialize<List<SiteRiskScore>>(row.RiskScoresJson, JsonSerializerConfiguration.Default) ?? []);
        AggregateReflectionHydrator.SetField(agg, "_anomalies",
            JsonSerializer.Deserialize<List<DetectedAnomaly>>(row.AnomaliesJson, JsonSerializerConfiguration.Default) ?? []);
        AggregateReflectionHydrator.SetField(agg, "_recommendations",
            JsonSerializer.Deserialize<List<SafetyRecommendation>>(row.RecommendationsJson, JsonSerializerConfiguration.Default) ?? []);
        AggregateReflectionHydrator.SetField(agg, "_historyEntries", new List<GlobalSafetyInsightsHistoryEntry>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by every usp_GlobalSafetyInsights_* query proc.</summary>
    private sealed record SnapshotRow
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
        public required int TotalRecordableIncidents { get; init; }
        public required int TotalLostTimeIncidents { get; init; }
        public required decimal TotalHoursWorked { get; init; }
        public required string RiskScoresJson { get; init; }
        public required string AnomaliesJson { get; init; }
        public required string RecommendationsJson { get; init; }
        public required DateTime ComputedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
