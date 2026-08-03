// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Billing.Domain
//  Layer:          Tests (Unit)
//  File:           UsageOverageCalculatorTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Unit tests for UsageOverageCalculator -- Phase 7 §3.1/§3.3 usage-based overage
//                  math, the highest-risk-of-being-wrong logic in the Billing module.
// ============================================================================
using PlantSafety.Domain.Billing.Metering.Aggregates;
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.Domain.Billing.Metering.Services;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using PlantSafety.Domain.Billing.Subscription.Catalog;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Tests.Billing.Domain;

public sealed class UsageOverageCalculatorTests : UnitTestBase
{
    private static UsageMeteringSnapshot BuildSnapshot(IReadOnlyDictionary<UsageMetricType, decimal> values) =>
        UsageMeteringSnapshot.Capture(TenantId.New(), new BillingPeriod(2026, 8), values, UtcTimestamp.Now());

    [Fact]
    public void ComputeOne_UsageWithinLimit_ProducesZeroOverage()
    {
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal> { [UsageMetricType.UserCount] = 10m });

        var result = UsageOverageCalculator.ComputeOne(snapshot, SubscriptionPlanCatalog.Standard, UsageMetricType.UserCount, "USD");

        result.RecordedUsage.Should().Be(10m);
        result.PlanLimit.Should().Be(25m);
        result.OverageQuantity.Should().Be(0m);
        result.OverageCharge.Amount.Should().Be(0m);
    }

    [Fact]
    public void ComputeOne_UsageExceedsLimit_ComputesPositiveOverageAndCharge()
    {
        // Standard plan: MaxUsers = 25, UserCount overage rate = $15/unit.
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal> { [UsageMetricType.UserCount] = 37m });

        var result = UsageOverageCalculator.ComputeOne(snapshot, SubscriptionPlanCatalog.Standard, UsageMetricType.UserCount, "USD");

        result.OverageQuantity.Should().Be(12m);
        result.OverageCharge.Amount.Should().Be(180.00m); // 12 * $15
    }

    [Fact]
    public void ComputeOne_UnlimitedMetric_NeverProducesOverage()
    {
        // Enterprise plan: MaxUsers = null (unlimited).
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal> { [UsageMetricType.UserCount] = 1_000_000m });

        var result = UsageOverageCalculator.ComputeOne(snapshot, SubscriptionPlanCatalog.Enterprise, UsageMetricType.UserCount, "USD");

        result.PlanLimit.Should().BeNull();
        result.OverageQuantity.Should().Be(0m);
        result.OverageCharge.Amount.Should().Be(0m);
    }

    [Fact]
    public void ComputeOne_MetricNotRecorded_TreatsAsZeroUsage()
    {
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal>());

        var result = UsageOverageCalculator.ComputeOne(snapshot, SubscriptionPlanCatalog.Standard, UsageMetricType.UserCount, "USD");

        result.RecordedUsage.Should().Be(0m);
        result.OverageQuantity.Should().Be(0m);
    }

    [Fact]
    public void ComputeOne_StorageBytes_ComputesPerGibOverageWithCeilingRounding()
    {
        // Standard plan: StorageQuotaBytes = 25 GiB, rate = $5 per GiB, ceiling-rounded units.
        var quotaBytes = 25L * 1024 * 1024 * 1024;
        var overBy1_5Gib = quotaBytes + (long)(1.5 * 1024 * 1024 * 1024);

        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal> { [UsageMetricType.StorageBytes] = overBy1_5Gib });

        var result = UsageOverageCalculator.ComputeOne(snapshot, SubscriptionPlanCatalog.Standard, UsageMetricType.StorageBytes, "USD");

        result.OverageQuantity.Should().Be(1.5m * 1024 * 1024 * 1024);
        // 1.5 GiB rounds up to 2 billable GiB units @ $5 = $10.
        result.OverageCharge.Amount.Should().Be(10.00m);
    }

    [Fact]
    public void ComputeAll_ReturnsOneResultPerDefinedMetric()
    {
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal>());

        var results = UsageOverageCalculator.ComputeAll(snapshot, SubscriptionPlanCatalog.Professional, "USD");

        results.Should().HaveCount(UsageMetricType.All.Count);
    }
}
