// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Analytics
//  Layer:          Tests (Unit)
//  File:           CultureAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Culture aggregate root in the Analytics bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Analytics.Culture.Aggregates;
using PlantSafety.Domain.Analytics.Culture.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Domain.Analytics;

/// <summary>Unit tests for <see cref="Culture"/> aggregate in the Analytics bounded context.</summary>
public sealed class CultureAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
               .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(CultureStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseCultureCreatedEvent()
    {
        var agg = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<CultureCreatedEvent>();
    }

    [Fact]
    public void Create_Event_SchemaVersion_ShouldBe2026()
    {
        var agg = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<CultureCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        a1.CultureId.Should().NotBe(a2.CultureId);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmpty()
    {
        var agg = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }
}
