// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Domain
//  Layer:          Tests (Unit)
//  File:           WasteContainerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Lifecycle/invariant tests for the WasteContainer aggregate root: open, close,
//                  ship, RCRA 90/180-day accumulation clock, and invalid transition guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Events;
using Agg = PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer;

namespace PlantSafety.Tests.HazardousWasteManagement.Domain;

public sealed class WasteContainerTests : UnitTestBase
{
    private static Agg OpenContainer(int regulatoryTimeLimitDays = 90) =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            accumulationAreaId: Guid.NewGuid(), wasteStreamId: Guid.NewGuid(),
            containerLabel: "DRUM-001", capacityGallons: 55m, regulatoryTimeLimitDays: regulatoryTimeLimitDays);

    [Fact]
    public void Create_WithValidData_OpensContainerAndRaisesEvent()
    {
        var container = OpenContainer();

        container.Status.Should().Be(WasteContainerStatus.Open);
        container.ContainerLabel.Should().Be("DRUM-001");
        container.CapacityGallons.Should().Be(55m);
        container.RegulatoryTimeLimitDays.Should().Be(90);
        container.OpenedAtUtc.Should().Be(FixedTimestamp);
        container.ClosedAtUtc.Should().BeNull();

        container.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccumulationContainerOpenedEvent>();
    }

    [Fact]
    public void Close_FromOpen_TransitionsAndSetsClosedTimestamp()
    {
        var container = OpenContainer();
        container.ClearDomainEvents();

        container.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        container.Status.Should().Be(WasteContainerStatus.Closed);
        container.ClosedAtUtc.Should().Be(FixedTimestamp);
        container.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccumulationContainerClosedEvent>();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsInvalidOperationException()
    {
        var container = OpenContainer();
        container.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => container.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkShipped_FromClosed_TransitionsAndRaisesEvent()
    {
        var container = OpenContainer();
        container.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        container.ClearDomainEvents();
        var manifestId = Guid.NewGuid();

        container.MarkShipped(manifestId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        container.Status.Should().Be(WasteContainerStatus.Shipped);
        container.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<WasteContainerShippedEvent>();
    }

    [Fact]
    public void MarkShipped_WhileStillOpen_ThrowsInvalidOperationException()
    {
        var container = OpenContainer();

        var act = () => container.MarkShipped(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TimeLimitDeadlineUtc_ReturnsOpenedAtUtcPlusRegulatoryDays()
    {
        var container = OpenContainer(regulatoryTimeLimitDays: 90);

        container.TimeLimitDeadlineUtc().Should().Be(FixedUtcNow.AddDays(90));
    }

    [Fact]
    public void IsPastTimeLimit_WhenOpenAndPastDeadline_ReturnsTrue()
    {
        var container = OpenContainer(regulatoryTimeLimitDays: 90);

        container.IsPastTimeLimit(FixedUtcNow.AddDays(91)).Should().BeTrue();
    }

    [Fact]
    public void IsPastTimeLimit_WhenOpenAndBeforeDeadline_ReturnsFalse()
    {
        var container = OpenContainer(regulatoryTimeLimitDays: 90);

        container.IsPastTimeLimit(FixedUtcNow.AddDays(10)).Should().BeFalse();
    }

    [Fact]
    public void IsPastTimeLimit_WhenClosed_ReturnsFalseEvenPastDeadline()
    {
        var container = OpenContainer(regulatoryTimeLimitDays: 90);
        container.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        container.IsPastTimeLimit(FixedUtcNow.AddDays(200)).Should().BeFalse();
    }
}
