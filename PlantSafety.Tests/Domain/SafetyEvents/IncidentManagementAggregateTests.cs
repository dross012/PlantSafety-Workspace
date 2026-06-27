// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.SafetyEvents
//  Layer:          Tests (Unit)
//  File:           IncidentManagementAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for IncidentManagement aggregate — invariants, event emission, status, and StrongId.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Domain.SafetyEvents;

/// <summary>
/// Unit tests for the <see cref="IncidentManagement"/> aggregate root.
/// Validates creation invariants, domain event emission, and SchemaVersion.
/// </summary>
public sealed class IncidentManagementAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_WithValidArgs_ShouldReturnAggregate()
    {
        var agg = IncidentManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = IncidentManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(IncidentManagementStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseIncidentManagementCreatedEvent()
    {
        var agg = IncidentManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<IncidentManagementCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = IncidentManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<IncidentManagementCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = IncidentManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.IncidentManagementId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyEventList()
    {
        var agg = IncidentManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }
}
