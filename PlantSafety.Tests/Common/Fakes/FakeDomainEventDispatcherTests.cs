// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.Fakes
//  Layer:          Tests (Unit)
//  File:           FakeDomainEventDispatcherTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests verifying FakeDomainEventDispatcher captures and clears events.
// ============================================================================
using PlantSafety.Tests.Common.Fakes;

namespace PlantSafety.Tests.Common.Fakes;

public sealed class FakeDomainEventDispatcherTests : UnitTestBase
{
    [Fact]
    public void DispatchAndClear_ShouldCaptureEvents()
    {
        var dispatcher = new FakeDomainEventDispatcher();
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        dispatcher.DispatchAndClear(agg);
        dispatcher.Of<AlertCreatedEvent>().Should().HaveCount(1);
    }

    [Fact]
    public void Single_ShouldReturnSingleEvent()
    {
        var dispatcher = new FakeDomainEventDispatcher();
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        dispatcher.DispatchAndClear(agg);
        var evt = dispatcher.Single<AlertCreatedEvent>();
        evt.Should().NotBeNull();
    }

    [Fact]
    public void DispatchAndClear_ShouldClearAggregateEvents()
    {
        var dispatcher = new FakeDomainEventDispatcher();
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        dispatcher.DispatchAndClear(agg);
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Of_WithNoMatchingEvents_ShouldReturnEmpty()
    {
        var dispatcher = new FakeDomainEventDispatcher();
        dispatcher.Of<AlertCreatedEvent>().Should().BeEmpty();
    }
}
