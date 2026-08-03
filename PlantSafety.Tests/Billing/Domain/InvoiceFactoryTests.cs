// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Billing.Domain
//  Layer:          Tests (Unit)
//  File:           InvoiceFactoryTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Unit tests for InvoiceFactory.GenerateForPeriod -- end-to-end invoice line-item
//                  generation composing overage math and discount/credit application. Highest-
//                  risk-of-being-wrong logic in the Billing module per this task's own scope note.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.Enums;
using PlantSafety.Domain.Billing.BillingEngine.Factories;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects;
using PlantSafety.Domain.Billing.Metering.Aggregates;
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using PlantSafety.Domain.Billing.Subscription.Catalog;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Tests.Billing.Domain;

public sealed class InvoiceFactoryTests : UnitTestBase
{
    private static readonly TenantId Tenant = TenantId.New();

    private static UsageMeteringSnapshot BuildSnapshot(IReadOnlyDictionary<UsageMetricType, decimal> values) =>
        UsageMeteringSnapshot.Capture(Tenant, new BillingPeriod(2026, 8), values, UtcTimestamp.Now());

    [Fact]
    public void GenerateForPeriod_NoOverage_ProducesOnlyBasePlanLineItem()
    {
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal> { [UsageMetricType.UserCount] = 5m });

        var invoice = InvoiceFactory.GenerateForPeriod(Tenant, SubscriptionPlanCatalog.Standard, snapshot, [], UtcTimestamp.Now());

        invoice.LineItems.Should().ContainSingle(li => li.Type == LineItemType.BasePlan);
        invoice.LineItems.Should().NotContain(li => li.Type == LineItemType.Overage);
        invoice.Subtotal.Amount.Should().Be(499.00m);
        invoice.TotalDue.Amount.Should().Be(499.00m);
        invoice.Status.Code.Should().Be("Draft");
    }

    [Fact]
    public void GenerateForPeriod_WithOverage_AddsOverageLineItemsAndIncreasesTotalDue()
    {
        // Standard plan: MaxUsers=25 (rate $15/unit), MaxScheduledTasksPerMonth=500 (rate $0.02/unit).
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal>
        {
            [UsageMetricType.UserCount] = 30m,          // 5 over -> $75
            [UsageMetricType.ScheduledTasks] = 600m,     // 100 over -> $2.00
        });

        var invoice = InvoiceFactory.GenerateForPeriod(Tenant, SubscriptionPlanCatalog.Standard, snapshot, [], UtcTimestamp.Now());

        invoice.LineItems.Should().Contain(li => li.Type == LineItemType.Overage && li.Description.Contains("User Count"));
        invoice.LineItems.Should().Contain(li => li.Type == LineItemType.Overage && li.Description.Contains("Scheduled Tasks"));

        // 499 (base) + 75 (user overage) + 2.00 (scheduled task overage) = 576.00
        invoice.Subtotal.Amount.Should().Be(576.00m);
        invoice.TotalDue.Amount.Should().Be(576.00m);
    }

    [Fact]
    public void GenerateForPeriod_WithDiscountsAndCredits_ReducesTotalDueBelowSubtotal()
    {
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal> { [UsageMetricType.UserCount] = 5m });
        var discount = DiscountOrCredit.Percentage("Launch promo", 20m);

        var invoice = InvoiceFactory.GenerateForPeriod(Tenant, SubscriptionPlanCatalog.Standard, snapshot, [discount], UtcTimestamp.Now());

        invoice.Subtotal.Amount.Should().Be(499.00m);
        invoice.TotalDue.Amount.Should().Be(399.20m); // 499 - 20% = 399.20
        invoice.LineItems.Should().ContainSingle(li => li.Type == LineItemType.Discount);
    }

    [Fact]
    public void GenerateForPeriod_TenantIdAndPeriodMatchSnapshot()
    {
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal>());

        var invoice = InvoiceFactory.GenerateForPeriod(Tenant, SubscriptionPlanCatalog.Trial, snapshot, [], UtcTimestamp.Now());

        invoice.TenantId.Should().Be(Tenant);
        invoice.Period.Should().Be(new BillingPeriod(2026, 8));
    }

    [Fact]
    public void GenerateForPeriod_UnlimitedPlan_UserOverageNeverBilled()
    {
        var snapshot = BuildSnapshot(new Dictionary<UsageMetricType, decimal> { [UsageMetricType.UserCount] = 10_000m });

        var invoice = InvoiceFactory.GenerateForPeriod(Tenant, SubscriptionPlanCatalog.Enterprise, snapshot, [], UtcTimestamp.Now());

        invoice.LineItems.Should().NotContain(li => li.Type == LineItemType.Overage && li.Description.Contains("User Count"));
    }
}
