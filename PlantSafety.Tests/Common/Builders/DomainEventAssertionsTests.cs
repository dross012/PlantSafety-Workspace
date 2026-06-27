// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.Builders
//  Layer:          Tests (Unit)
//  File:           DomainEventAssertionsTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DomainEventAssertions static helper class.
// ============================================================================
namespace PlantSafety.Tests.Common.Builders;

public sealed class DomainEventAssertionsTests : UnitTestBase
{
    [Fact]
    public void ShouldContainSingle_WithMatchingEvent_ShouldReturnEvent()
    {
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var events = (IReadOnlyCollection<IDomainEvent>)agg.DomainEvents
            .Cast<IDomainEvent>().ToList().AsReadOnly();
        var evt = DomainEventAssertions.ShouldContainSingle<AlertCreatedEvent>(events);
        evt.Should().NotBeNull();
    }

    [Fact]
    public void ShouldHaveSchemaVersion2026_WithCorrectVersion_ShouldNotThrow()
    {
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.Cast<IDomainEvent>().First();
        var act = () => DomainEventAssertions.ShouldHaveSchemaVersion2026(evt);
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveValidVersion_WithVersionAtLeast1_ShouldNotThrow()
    {
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.Cast<IDomainEvent>().First();
        var act = () => DomainEventAssertions.ShouldHaveValidVersion(evt);
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveOccurredAt_WithMatchingTimestamp_ShouldNotThrow()
    {
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.Cast<IDomainEvent>().First();
        var act = () => DomainEventAssertions.ShouldHaveOccurredAt(evt, FixedTimestamp);
        act.Should().NotThrow();
    }
}
