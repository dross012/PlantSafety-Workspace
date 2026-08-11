// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Billing.Domain
//  Layer:          Tests (Unit)
//  File:           MoneyTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the SharedKernel Money value type introduced for Phase 7 §3
//                  Billing.
// ============================================================================
namespace PlantSafety.Tests.Billing.Domain;

public sealed class MoneyTests : UnitTestBase
{
    [Fact]
    public void Constructor_RoundsToTwoDecimalPlaces()
    {
        var money = new Money(19.005m, "usd");

        money.Amount.Should().Be(19.01m); // AwayFromZero rounding
        money.Currency.Should().Be("USD"); // upper-cased
    }

    [Fact]
    public void Addition_SameCurrency_Succeeds()
    {
        var result = new Money(10m, "USD") + new Money(5.50m, "USD");
        result.Amount.Should().Be(15.50m);
    }

    [Fact]
    public void Addition_DifferentCurrency_Throws()
    {
        var act = () => new Money(10m, "USD") + new Money(5m, "EUR");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiplication_ByQuantity_ScalesAmount()
    {
        var result = new Money(15m, "USD") * 12m;
        result.Amount.Should().Be(180.00m);
    }

    [Fact]
    public void ClampToZero_NegativeAmount_ReturnsZero()
    {
        var result = new Money(-25m, "USD").ClampToZero();
        result.Amount.Should().Be(0m);
    }

    [Fact]
    public void ClampToZero_PositiveAmount_Unchanged()
    {
        var result = new Money(25m, "USD").ClampToZero();
        result.Amount.Should().Be(25m);
    }

    [Fact]
    public void Comparison_Operators_WorkAsExpected()
    {
        var lower = new Money(10m, "USD");
        var higher = new Money(20m, "USD");

        // Separate instances holding the same amount, not the same variable twice. Comparing a
        // variable to itself is what CS1718 flagged, and it was also the weaker test: it can pass
        // on reference identity alone, so it would not notice if <= and >= stopped honouring
        // value equality -- the exact property these operators exist to provide.
        var sameAsLower = new Money(10m, "USD");
        var sameAsHigher = new Money(20m, "USD");

        (lower < higher).Should().BeTrue();
        (higher > lower).Should().BeTrue();
        (lower <= sameAsLower).Should().BeTrue();
        (higher >= sameAsHigher).Should().BeTrue();

        // The strict operators must reject equal values, or <= and >= above would pass for a
        // type whose comparisons ignore equality entirely.
        (lower < sameAsLower).Should().BeFalse();
        (higher > sameAsHigher).Should().BeFalse();
    }
}
