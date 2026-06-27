// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.OperationalSafety
//  Layer:          Tests (Unit)
//  File:           AlertAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Alert aggregate root — invariants, state transitions, and domain event emission.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Operational.Alerts.Aggregates;
using PlantSafety.Domain.Operational.Alerts.Events;
using PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Domain.OperationalSafety;

/// <summary>
/// Unit tests for the <see cref="Alert"/> aggregate root.
/// Validates: creation invariants, domain event emission, SchemaVersion,
/// lifecycle status, and StrongId generation.
/// All tests run in under 50 ms — no I/O, no network.
/// </summary>
public sealed class AlertAggregateTests : UnitTestBase
{
    // ---- Create -------------------------------------------------------

    [Fact]
    public void Create_WithValidArgs_ShouldReturnAggregateWithStatusCreated()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        alert.Should().NotBeNull();
        alert.Status.Should().Be(AlertStatus.Created);
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyAlertId()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        alert.AlertId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedByToSuppliedUserId()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        alert.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtcToSuppliedTimestamp()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        alert.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    // ---- Domain Events ------------------------------------------------

    [Fact]
    public void Create_ShouldRaiseExactlyOneAlertCreatedEvent()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        var evt = alert.DomainEvents.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        evt.Should().NotBeNull();
    }

    [Fact]
    public void Create_RaisedEvent_ShouldHaveSchemaVersion2026()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        var evt = alert.DomainEvents.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_RaisedEvent_ShouldHaveVersionAtLeast1()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        var evt = alert.DomainEvents.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        evt.EventVersion.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void Create_RaisedEvent_OccurredAtUtc_ShouldMatchCreationTimestamp()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        var evt = alert.DomainEvents.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        evt.OccurredAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Create_RaisedEvent_ShouldCarryAlertId()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        var evt = alert.DomainEvents.ShouldRaiseSingleEvent<AlertCreatedEvent>();
        evt.AlertId.Should().Be(alert.AlertId);
    }

    // ---- ClearDomainEvents --------------------------------------------

    [Fact]
    public void ClearDomainEvents_ShouldResultInEmptyEventList()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        alert.ClearDomainEvents();

        alert.DomainEvents.Should().BeEmpty();
    }

    // ---- StrongId invariants ------------------------------------------

    [Fact]
    public void TwoAlerts_Created_ShouldHaveDifferentAlertIds()
    {
        var a1 = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        a1.AlertId.Should().NotBe(a2.AlertId);
    }

    // ---- DomainEvent count --------------------------------------------

    [Fact]
    public void Create_ShouldRaiseExactlyOneDomainEvent()
    {
        var alert = Alert.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

        alert.DomainEvents.Should().HaveCount(1);
    }
}
