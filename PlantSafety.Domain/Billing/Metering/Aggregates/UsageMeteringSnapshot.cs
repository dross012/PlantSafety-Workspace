// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Domain
//  File:           UsageMeteringSnapshot.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root capturing an immutable, point-in-time record of a tenant's usage
//                  across all six metered dimensions (UsageMetricType) for a single BillingPeriod.
//                  Produced by the Application layer's GetTenantUsageMeteringSummaryQuery (which
//                  composes the platform's existing data sources -- PlatformHealthDashboard,
//                  PredictiveScheduling, IBlobStorageService, DeadLetterEvents, CrossDomain.Users,
//                  TenantConfiguration.EnabledModules) and persisted so a later billing run can
//                  recompute the exact same invoice deterministically, independent of whether the
//                  underlying live data sources have since rolled off/aged out. Additive-only
//                  evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using PlantSafety.Domain.Billing.Metering.ValueObjects.Ids;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Billing.Metering.Aggregates;

/// <summary>Immutable, point-in-time snapshot of a tenant's usage across every metered dimension for a single billing period.</summary>
public sealed class UsageMeteringSnapshot
{
    private readonly Dictionary<string, decimal> _recordedValues;

    /// <summary>Strong-typed identity of this snapshot.</summary>
    public UsageMeteringSnapshotId UsageMeteringSnapshotId { get; private set; }

    /// <summary>The tenant this snapshot was recorded for.</summary>
    public TenantId TenantId { get; private set; }

    /// <summary>The calendar-month billing period this snapshot covers.</summary>
    public BillingPeriod Period { get; private set; }

    /// <summary>UTC instant this snapshot was captured.</summary>
    public UtcTimestamp CapturedAtUtc { get; private set; }

    /// <summary>Recorded usage values, keyed by <see cref="UsageMetricType.Code"/>.</summary>
    public IReadOnlyDictionary<string, decimal> RecordedValues => new ReadOnlyDictionary<string, decimal>(_recordedValues);

    // For rehydration only.
    private UsageMeteringSnapshot() { _recordedValues = []; }

    /// <summary>Captures a new UsageMeteringSnapshot from a fully-populated set of recorded metric values.</summary>
    public static UsageMeteringSnapshot Capture(
        TenantId tenantId, BillingPeriod period, IReadOnlyDictionary<UsageMetricType, decimal> recordedValues, UtcTimestamp capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(recordedValues);

        var snapshot = new UsageMeteringSnapshot
        {
            UsageMeteringSnapshotId = UsageMeteringSnapshotId.New(),
            TenantId = tenantId,
            Period = period,
            CapturedAtUtc = capturedAtUtc,
        };

        foreach (var (metric, value) in recordedValues)
        {
            if (value < 0)
                throw new ArgumentException($"UsageMeteringSnapshot recorded value for '{metric.Code}' must not be negative.", nameof(recordedValues));
            snapshot._recordedValues[metric.Code] = value;
        }

        return snapshot;
    }

    /// <summary>Returns the recorded value for the given metric, or zero if it was not recorded in this snapshot.</summary>
    public decimal GetValue(UsageMetricType metric) => _recordedValues.GetValueOrDefault(metric.Code, 0m);

    /// <summary>Rehydrates a UsageMeteringSnapshot from persisted state. Infrastructure-layer use only.</summary>
    public static UsageMeteringSnapshot Rehydrate(
        UsageMeteringSnapshotId id, TenantId tenantId, BillingPeriod period, UtcTimestamp capturedAtUtc,
        IReadOnlyDictionary<string, decimal> recordedValues)
    {
        var snapshot = new UsageMeteringSnapshot
        {
            UsageMeteringSnapshotId = id,
            TenantId = tenantId,
            Period = period,
            CapturedAtUtc = capturedAtUtc,
        };
        foreach (var (code, value) in recordedValues)
            snapshot._recordedValues[code] = value;
        return snapshot;
    }
}
