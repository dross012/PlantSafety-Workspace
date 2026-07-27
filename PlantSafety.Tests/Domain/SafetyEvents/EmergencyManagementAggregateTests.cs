// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.EmergencyManagement
//  Layer:          Tests (Unit)
//  File:           EmergencyManagementAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the EmergencyManagement aggregate in the SafetyEvents bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using EmergencyManagementAggregate = PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events;
using PlantSafety.Tests.Common.Extensions;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Enums;

namespace PlantSafety.Tests.SafetyEvents.EmergencyManagement;

/// <summary>Unit tests for <see cref="EmergencyManagement"/> aggregate in <c>SafetyEvents</c>.</summary>
public sealed class EmergencyManagementAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
               .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(EmergencyManagementStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEvent()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<EmergencyManagementCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<EmergencyManagementCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.EmergencyManagementId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedBy()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtc()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        a1.EmergencyManagementId.Should().NotBe(a2.EmergencyManagementId);
    }

    [Fact]
    public void Create_Event_OccurredAtUtc_ShouldMatchTimestamp()
    {
        var agg = EmergencyManagementAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<EmergencyManagementCreatedEvent>();
        evt.OccurredAtUtc.Should().Be(FixedTimestamp);
    }
}
