// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.PredictiveAnalytics
//  Layer:          Tests (Unit)
//  File:           PredictiveAnalyticsAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the PredictiveAnalytics aggregate in the Analytics bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Analytics.PredictiveAnalytics;

/// <summary>Unit tests for <see cref="PredictiveAnalytics"/> aggregate in <c>Analytics</c>.</summary>
public sealed class PredictiveAnalyticsAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
               .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(PredictiveAnalyticsStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEvent()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<PredictiveAnalyticsCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<PredictiveAnalyticsCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.PredictiveAnalyticsId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedBy()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtc()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        a1.PredictiveAnalyticsId.Should().NotBe(a2.PredictiveAnalyticsId);
    }

    [Fact]
    public void Create_Event_OccurredAtUtc_ShouldMatchTimestamp()
    {
        var agg = PredictiveAnalytics.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<PredictiveAnalyticsCreatedEvent>();
        evt.OccurredAtUtc.Should().Be(FixedTimestamp);
    }
}
