// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Infrastructure
//  File:           CognitiveRiskPredictionDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ICognitiveRiskPredictionRepository. All DB
//                  access via stored procedures in the [HumanFactorsCognitiveLoadAnalytics]
//                  schema. Aggregate reconstruction uses AggregateReflectionHydrator because
//                  CognitiveRiskPrediction exposes only a private "for rehydration only"
//                  constructor and private-setter properties. Mirrors
//                  EsgDisclosureReportDapperRepository's precedent for a single-row (no child
//                  collections) aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Events;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.HumanFactors.CognitiveLoadAnalytics.Persistence;

/// <summary>Dapper-backed repository for the CognitiveRiskPrediction aggregate root.</summary>
public sealed class CognitiveRiskPredictionDapperRepository(
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ISqlConnectionFactory connectionFactory,
    IOutboxWriter outboxWriter)
    : ICognitiveRiskPredictionRepository
{
    /// <inheritdoc/>
    public async Task<CognitiveRiskPrediction?> GetByIdAsync(CognitiveRiskPredictionId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("CognitiveRiskPredictionId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PredictionRow>(
            "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(CognitiveRiskPrediction aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("CognitiveRiskPredictionId", aggregate.CognitiveRiskPredictionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddGuid("WorkerUserId", aggregate.WorkerUserId.Value)
            .AddGuid("SourceLedgerId", aggregate.SourceLedgerId.Value)
            .AddString("PredictedRiskLevel", aggregate.PredictedRiskLevel.ToString(), 20)
            .AddDecimal("PredictionScore", aggregate.PredictionScore)
            .AddString("ModelVersion", aggregate.ModelVersion, 100)
            .AddString("PredictionBasis", aggregate.PredictionBasis, 1000)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("GeneratedAtUtc", aggregate.GeneratedAtUtc.Value)
            .AddGuid("GeneratedBy", aggregate.GeneratedBy.Value)
            .Build();

        using var conn = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(conn, "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_Insert]", parameters, cancellationToken: cancellationToken);

        DispatchDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(CognitiveRiskPrediction aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("CognitiveRiskPredictionId", aggregate.CognitiveRiskPredictionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("AcknowledgedAtUtc", aggregate.AcknowledgedAtUtc?.Value)
            .AddGuid("AcknowledgedBy", aggregate.AcknowledgedBy?.Value)
            .AddString("AcknowledgementNotes", aggregate.AcknowledgementNotes, 1000)
            .Build();

        using var conn = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteScalarAsync(conn, "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_UpdateAcknowledgement]", parameters, cancellationToken: cancellationToken);

        DispatchDomainEvents(aggregate);
    }

    private static CognitiveRiskPrediction Hydrate(PredictionRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<CognitiveRiskPrediction>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.CognitiveRiskPredictionId), CognitiveRiskPredictionId.FromGuid(row.CognitiveRiskPredictionId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.WorkerUserId), new UserId(row.WorkerUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.SourceLedgerId), CognitiveLoadLedgerId.FromGuid(row.SourceLedgerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.PredictedRiskLevel), Enum.Parse<CognitiveRiskLevel>(row.PredictedRiskLevel));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.PredictionScore), row.PredictionScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.ModelVersion), row.ModelVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.PredictionBasis), row.PredictionBasis);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.Status), Enum.Parse<CognitiveRiskPredictionStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.GeneratedAtUtc), UtcTimestamp.From(row.GeneratedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.GeneratedBy), new UserId(row.GeneratedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.AcknowledgedAtUtc), row.AcknowledgedAtUtc is { } a ? UtcTimestamp.From(a) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.AcknowledgedBy), row.AcknowledgedBy is { } b ? new UserId(b) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveRiskPrediction.AcknowledgementNotes), row.AcknowledgementNotes);
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ICognitiveLoadAnalyticsDomainEvent>());

        return agg;
    }

    private void DispatchDomainEvents(CognitiveRiskPrediction aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private sealed record PredictionRow
    {
        public required Guid CognitiveRiskPredictionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required Guid WorkerUserId { get; init; }
        public required Guid SourceLedgerId { get; init; }
        public required string PredictedRiskLevel { get; init; }
        public decimal PredictionScore { get; init; }
        public required string ModelVersion { get; init; }
        public required string PredictionBasis { get; init; }
        public required string Status { get; init; }
        public required DateTime GeneratedAtUtc { get; init; }
        public required Guid GeneratedBy { get; init; }
        public DateTime? AcknowledgedAtUtc { get; init; }
        public Guid? AcknowledgedBy { get; init; }
        public string? AcknowledgementNotes { get; init; }
    }
}
