// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity
//  Layer:          Infrastructure
//  File:           SensorIntegrityCheckDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed repository for the SensorIntegrityCheck aggregate root. Append-only
//                  (no UpdateAsync -- integrity-check history is immutable once recorded).
//                  Implements both the Domain write-side repository and the Application read-side
//                  repository.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Repositories;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Aggregates;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;

namespace PlantSafety.Infrastructure.CyberPhysicalSecurity.Persistence;

/// <summary>Dapper-backed repository for the <see cref="SensorIntegrityCheck"/> aggregate root.</summary>
public sealed class SensorIntegrityCheckDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SensorIntegrityCheckDapperRepository> logger)
    : ISensorIntegrityCheckRepository, ISensorIntegrityCheckReadRepository
{
    /// <inheritdoc/>
    public async Task<SensorIntegrityCheck?> GetByIdAsync(SensorIntegrityCheckId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("SensorIntegrityCheckId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<SensorIntegrityCheckRow>(
            "[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SensorIntegrityCheck>> GetHistoryForAssetAsync(OtAssetId otAssetId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("OtAssetId", otAssetId.Value).AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<SensorIntegrityCheckRow>(
            "[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetHistoryForAsset]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    async Task<IReadOnlyList<SensorIntegrityCheckDto>> ISensorIntegrityCheckReadRepository.GetHistoryForAssetAsync(Guid otAssetId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("OtAssetId", otAssetId).AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<SensorIntegrityCheckRow>(
            "[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetHistoryForAsset]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OtAssetId>> ListAssetsWithChecksDueAsync(Guid tenantId, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddDateTime("AsOfUtc", asOfUtc).Build();
        var rows = await queryExecutor.QueryAsync<Guid>("[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetAssetsWithChecksDue]", parameters, cancellationToken);

        return rows.Select(OtAssetId.FromGuid).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(SensorIntegrityCheck aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add SensorIntegrityCheck {SensorIntegrityCheckId}", aggregate.SensorIntegrityCheckId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SensorIntegrityCheckId", aggregate.SensorIntegrityCheckId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddGuid("OtAssetId", aggregate.OtAssetId.Value)
            .AddString("CheckType", aggregate.CheckType.ToString(), 20)
            .AddString("Result", aggregate.Result.ToString(), 20)
            .AddString("Details", aggregate.Details, 2000)
            .AddDateTime("CheckedAtUtc", aggregate.CheckedAtUtc.Value)
            .AddDateTime("NextCheckDueUtc", aggregate.NextCheckDueUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_SensorIntegrityCheck_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());

        aggregate.ClearDomainEvents();
    }

    private static SensorIntegrityCheck Hydrate(SensorIntegrityCheckRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<SensorIntegrityCheck>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.SensorIntegrityCheckId), SensorIntegrityCheckId.FromGuid(row.SensorIntegrityCheckId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.OtAssetId), OtAssetId.FromGuid(row.OtAssetId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.CheckType), Enum.Parse<SensorIntegrityCheckType>(row.CheckType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.Result), Enum.Parse<SensorIntegrityCheckResult>(row.Result));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.Details), row.Details);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.CheckedAtUtc), UtcTimestamp.From(row.CheckedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.NextCheckDueUtc), row.NextCheckDueUtc.HasValue ? UtcTimestamp.From(row.NextCheckDueUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SensorIntegrityCheck.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ISensorIntegrityCheckDomainEvent>());

        return agg;
    }

    private static SensorIntegrityCheckDto ToDto(SensorIntegrityCheckRow row) => new(
        row.SensorIntegrityCheckId, row.TenantId, row.SiteId, row.OtAssetId, row.CheckType, row.Result,
        row.Details, row.CheckedAtUtc, row.NextCheckDueUtc);

    /// <summary>Flat read-model row shape returned by the SensorIntegrityCheck stored procedures.</summary>
    private sealed record SensorIntegrityCheckRow
    {
        public required Guid SensorIntegrityCheckId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SiteId { get; init; }
        public required Guid OtAssetId { get; init; }
        public required string CheckType { get; init; }
        public required string Result { get; init; }
        public required string Details { get; init; }
        public required DateTime CheckedAtUtc { get; init; }
        public DateTime? NextCheckDueUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
