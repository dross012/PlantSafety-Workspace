// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Domain
//  Layer:          Tests (Unit)
//  File:           AccumulationAreaTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Lifecycle/invariant tests for the AccumulationArea aggregate root: register,
//                  flag time-limit warning, deactivate, and invalid transition guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Enums;
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Events;
using Agg = PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea;

namespace PlantSafety.Tests.HazardousWasteManagement.Domain;

public sealed class AccumulationAreaTests : UnitTestBase
{
    private static Agg RegisterArea() =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            areaName: "Building 3 Satellite Area",
            locationDescription: "Adjacent to Loading Dock B",
            timeLimitDays: 90);

    [Fact]
    public void Create_WithValidData_RegistersActiveAreaAndRaisesEvent()
    {
        var area = RegisterArea();

        area.Status.Should().Be(AccumulationAreaStatus.Active);
        area.AreaName.Should().Be("Building 3 Satellite Area");
        area.TimeLimitDays.Should().Be(90);

        area.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccumulationAreaRegisteredEvent>();
    }

    [Fact]
    public void FlagTimeLimitApproaching_FromActive_SetsWarningStatusAndRaisesEvent()
    {
        var area = RegisterArea();
        area.ClearDomainEvents();
        var containerId = Guid.NewGuid();
        var deadline = FixedUtcNow.AddDays(3);

        area.FlagTimeLimitApproaching(containerId, deadline, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        area.Status.Should().Be(AccumulationAreaStatus.TimeLimitWarning);
        area.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccumulationTimeLimitApproachingEvent>();
    }

    [Fact]
    public void Deactivate_FromActive_TransitionsAndRaisesEvent()
    {
        var area = RegisterArea();
        area.ClearDomainEvents();

        area.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        area.Status.Should().Be(AccumulationAreaStatus.Inactive);
        area.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccumulationAreaDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_FromTimeLimitWarning_IsAllowed()
    {
        var area = RegisterArea();
        area.FlagTimeLimitApproaching(Guid.NewGuid(), FixedUtcNow.AddDays(1), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => area.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().NotThrow();
        area.Status.Should().Be(AccumulationAreaStatus.Inactive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ThrowsInvalidOperationException()
    {
        var area = RegisterArea();
        area.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => area.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
