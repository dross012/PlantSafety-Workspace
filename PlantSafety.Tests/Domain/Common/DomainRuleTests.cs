// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Common
//  Layer:          Tests (Unit)
//  File:           DomainRuleTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for IDomainRule and BusinessRuleValidator base mechanics.
// ============================================================================
namespace PlantSafety.Tests.Domain.Common;

public sealed class DomainRuleTests : UnitTestBase
{
    private sealed class AlwaysPassRule : IDomainRule
    {
        public bool IsBroken() => false;
        public string Message => "This rule never breaks";
    }

    private sealed class AlwaysFailRule : IDomainRule
    {
        public bool IsBroken() => true;
        public string Message => "This rule always breaks";
    }

    [Fact]
    public void AlwaysPassRule_IsBroken_ShouldReturnFalse() =>
        new AlwaysPassRule().IsBroken().Should().BeFalse();

    [Fact]
    public void AlwaysFailRule_IsBroken_ShouldReturnTrue() =>
        new AlwaysFailRule().IsBroken().Should().BeTrue();

    [Fact]
    public void CheckRule_WithBrokenRule_ShouldThrow() =>
        FluentActions.Invoking(() => BusinessRuleValidator.Check(
                new AlwaysFailRule(), TestCorrelationId, TestCausationId))
                     .Should().Throw<BusinessRuleViolationException>();

    [Fact]
    public void CheckRule_WithPassingRule_ShouldNotThrow() =>
        FluentActions.Invoking(() => BusinessRuleValidator.Check(
                new AlwaysPassRule(), TestCorrelationId, TestCausationId))
                     .Should().NotThrow();
}
