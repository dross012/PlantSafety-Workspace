// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain
//  Layer:          Tests (Unit)
//  File:           DischargePointAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the DischargePoint aggregate root.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Exceptions;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain;

public sealed class DischargePointAggregateTests : UnitTestBase
{
    private static Agg RegisteredPoint() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        siteId: Guid.NewGuid(),
        dischargePermitId: Guid.NewGuid(),
        pointCode: "Outfall 001",
        pointName: "Main Process Outfall",
        receivingWaterBody: "Willow Creek",
        outfall: OutfallType.Direct,
        latitude: 37.5m,
        longitude: -122.1m);

    [Fact]
    public void Create_WithValidData_RegistersActivePointAndRaisesEvent()
    {
        var point = RegisteredPoint();

        point.Status.Should().Be(DischargePointStatus.Active);
        point.PointCode.Should().Be("Outfall 001");
        point.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Deactivate_WhenActive_TransitionsToInactive()
    {
        var point = RegisteredPoint();

        point.Deactivate("Seasonal shutdown.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        point.Status.Should().Be(DischargePointStatus.Inactive);
    }

    [Fact]
    public void Reactivate_WhenInactive_TransitionsToActive()
    {
        var point = RegisteredPoint();
        point.Deactivate(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        point.Reactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        point.Status.Should().Be(DischargePointStatus.Active);
    }

    [Fact]
    public void Reactivate_WhenAlreadyActive_ThrowsInvalidStatusTransition()
    {
        var point = RegisteredPoint();

        var act = () => point.Reactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargePointInvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Decommission_FromActiveOrInactive_TransitionsToDecommissioned(bool deactivateFirst)
    {
        var point = RegisteredPoint();
        if (deactivateFirst)
            point.Deactivate(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        point.Decommission("End of life.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        point.Status.Should().Be(DischargePointStatus.Decommissioned);
    }

    [Fact]
    public void Decommission_WhenAlreadyDecommissioned_ThrowsInvalidStatusTransition()
    {
        var point = RegisteredPoint();
        point.Decommission(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => point.Decommission(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargePointInvalidStatusTransitionException>();
    }

    [Fact]
    public void Deactivate_WhenDecommissioned_ThrowsInvalidStatusTransition()
    {
        var point = RegisteredPoint();
        point.Decommission(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => point.Deactivate(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargePointInvalidStatusTransitionException>();
    }
}
