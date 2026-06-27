// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.Extensions
//  Layer:          Tests (Unit)
//  File:           DomainEventExtensionsTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DomainEventExtensions helper assertions.
// ============================================================================
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Common.Extensions;

public sealed class DomainEventExtensionsTests : UnitTestBase
{
    [Fact]
    public void ShouldRaiseSingleEvent_WithCorrectType_ShouldReturnEvent()
    {
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        evt.Should().NotBeNull();
    }

    [Fact]
    public void ShouldRaiseSingleEvent_WithNoEvents_ShouldThrow()
    {
        var events = Array.Empty<IDomainEvent>();
        var act = () => events.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void WithSchemaVersion2026_ShouldValidateSchemaVersion()
    {
        var agg = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        var returned = evt.WithSchemaVersion2026();
        returned.Should().Be(evt);
    }
}
