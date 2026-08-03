// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Billing.Domain
//  Layer:          Tests (Unit)
//  File:           InvoiceCalculationRulesTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Unit tests for InvoiceCalculationRules -- subtotal computation and sequential
//                  discount/credit application. Highest-risk-of-being-wrong logic in the Billing
//                  module per this task's own scope note.
// ============================================================================
using PlantSafety.Domain.Billing.BillingEngine.Rules;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects;

namespace PlantSafety.Tests.Billing.Domain;

public sealed class InvoiceCalculationRulesTests : UnitTestBase
{
    [Fact]
    public void ComputeSubtotal_SumsAllLineItemAmounts()
    {
        var lineItems = new List<InvoiceLineItem>
        {
            InvoiceLineItem.ForBasePlan("Standard", new Money(499m, "USD")),
            InvoiceLineItem.ForOverage("UserCount", 12m, new Money(15m, "USD"), new Money(180m, "USD"))!,
        };

        var subtotal = InvoiceCalculationRules.ComputeSubtotal(lineItems, "USD");

        subtotal.Amount.Should().Be(679.00m);
    }

    [Fact]
    public void ApplyDiscountsAndCredits_NoDiscounts_TotalDueEqualsSubtotal()
    {
        var result = InvoiceCalculationRules.ApplyDiscountsAndCredits(new Money(500m, "USD"), []);

        result.DiscountLineItems.Should().BeEmpty();
        result.TotalDue.Amount.Should().Be(500m);
    }

    [Fact]
    public void ApplyDiscountsAndCredits_SinglePercentageDiscount_ReducesTotalDueCorrectly()
    {
        var discount = DiscountOrCredit.Percentage("10% loyalty discount", 10m);

        var result = InvoiceCalculationRules.ApplyDiscountsAndCredits(new Money(500m, "USD"), [discount]);

        result.DiscountLineItems.Should().HaveCount(1);
        result.DiscountLineItems[0].Amount.Amount.Should().Be(-50.00m);
        result.TotalDue.Amount.Should().Be(450.00m);
    }

    [Fact]
    public void ApplyDiscountsAndCredits_SequentialDiscounts_EachAppliesAgainstRunningBalance()
    {
        // 500 -> 10% off -> 450 -> then $100 fixed off -> 350.
        var percentageDiscount = DiscountOrCredit.Percentage("10% off", 10m);
        var fixedDiscount = DiscountOrCredit.FixedDiscount("$100 credit note", new Money(100m, "USD"));

        var result = InvoiceCalculationRules.ApplyDiscountsAndCredits(new Money(500m, "USD"), [percentageDiscount, fixedDiscount]);

        result.DiscountLineItems.Should().HaveCount(2);
        result.DiscountLineItems[0].Amount.Amount.Should().Be(-50.00m);
        result.DiscountLineItems[1].Amount.Amount.Should().Be(-100.00m);
        result.TotalDue.Amount.Should().Be(350.00m);
    }

    [Fact]
    public void ApplyDiscountsAndCredits_DiscountLargerThanRemainingBalance_ClampsToZero_NeverNegative()
    {
        var hugeCredit = DiscountOrCredit.AccountCredit("Goodwill credit", new Money(1000m, "USD"));

        var result = InvoiceCalculationRules.ApplyDiscountsAndCredits(new Money(200m, "USD"), [hugeCredit]);

        result.DiscountLineItems[0].Amount.Amount.Should().Be(-200.00m); // capped to the remaining subtotal, not -1000.
        result.TotalDue.Amount.Should().Be(0m);
    }

    [Fact]
    public void ApplyDiscountsAndCredits_MultipleCreditsExceedingBalance_SecondCreditIsFullyClamped()
    {
        var credit1 = DiscountOrCredit.AccountCredit("Credit 1", new Money(150m, "USD"));
        var credit2 = DiscountOrCredit.AccountCredit("Credit 2", new Money(150m, "USD"));

        var result = InvoiceCalculationRules.ApplyDiscountsAndCredits(new Money(200m, "USD"), [credit1, credit2]);

        result.DiscountLineItems[0].Amount.Amount.Should().Be(-150.00m); // 200 -> 50 remaining.
        result.DiscountLineItems[1].Amount.Amount.Should().Be(-50.00m);  // clamped to the remaining 50, not -150.
        result.TotalDue.Amount.Should().Be(0m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void Percentage_OutOfRangeValue_Throws(decimal invalidPercentage)
    {
        var act = () => DiscountOrCredit.Percentage("bad", invalidPercentage);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
