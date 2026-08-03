// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Infrastructure
//  File:           UsageMeteringSnapshotDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IUsageMeteringSnapshotRepository. Recorded
//                  metric values are persisted as a JSON dictionary (mirroring Tenant's own
//                  ConfigurationJson convention on TenantDapperRepository) rather than a normalized
//                  child table, since the set of metrics is small, closed, and always read/written
//                  as a single unit alongside its snapshot. Additive-only evolution.
// ============================================================================
using System.Text.Json;
using PlantSafety.Domain.Billing.Metering.Aggregates;
using PlantSafety.Domain.Billing.Metering.Interfaces;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Infrastructure.Billing.Metering.Persistence;

/// <summary>Dapper-backed repository for <see cref="UsageMeteringSnapshot"/> persistence.</summary>
public sealed class UsageMeteringSnapshotDapperRepository(
    ISqlConnectionFactory connectionFactory, DapperQueryExecutor queryExecutor, DapperCommandExecutor commandExecutor,
    ILogger<UsageMeteringSnapshotDapperRepository> logger)
    : IUsageMeteringSnapshotRepository
{
    /// <inheritdoc/>
    public async Task SaveAsync(UsageMeteringSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Saving UsageMeteringSnapshot {SnapshotId} for Tenant {TenantId} Period {Period}",
            snapshot.UsageMeteringSnapshotId.Value, snapshot.TenantId.Value, snapshot.Period);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("UsageMeteringSnapshotId", snapshot.UsageMeteringSnapshotId.Value)
            .AddGuid("TenantId", snapshot.TenantId.Value)
            .AddInt("PeriodYear", snapshot.Period.Year)
            .AddInt("PeriodMonth", snapshot.Period.Month)
            .AddDateTime("CapturedAtUtc", snapshot.CapturedAtUtc.Value)
            .AddString("RecordedValuesJson", JsonSerializer.Serialize(snapshot.RecordedValues), size: -1)
            .Build();

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        await commandExecutor.ExecuteAsync(conn, "[Billing].[usp_UsageMeteringSnapshot_Upsert]", parameters, tx, cancellationToken: cancellationToken);

        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<UsageMeteringSnapshot?> GetAsync(TenantId tenantId, BillingPeriod period, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId.Value)
            .AddInt("PeriodYear", period.Year)
            .AddInt("PeriodMonth", period.Month)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<UsageMeteringSnapshotRow>(
            "[Billing].[usp_UsageMeteringSnapshot_GetByTenantPeriod]", parameters, cancellationToken);

        if (row is null)
            return null;

        var recordedValues = JsonSerializer.Deserialize<Dictionary<string, decimal>>(row.RecordedValuesJson) ?? [];

        return UsageMeteringSnapshot.Rehydrate(
            Domain.Billing.Metering.ValueObjects.Ids.UsageMeteringSnapshotId.FromGuid(row.UsageMeteringSnapshotId),
            TenantId.FromGuid(row.TenantId),
            new BillingPeriod(row.PeriodYear, row.PeriodMonth),
            PlantSafety.SharedKernel.Primitives.ValueTypes.UtcTimestamp.From(row.CapturedAtUtc),
            recordedValues);
    }

    private sealed record UsageMeteringSnapshotRow
    {
        public required Guid UsageMeteringSnapshotId { get; init; }
        public required Guid TenantId { get; init; }
        public required int PeriodYear { get; init; }
        public required int PeriodMonth { get; init; }
        public required DateTime CapturedAtUtc { get; init; }
        public required string RecordedValuesJson { get; init; }
    }
}
