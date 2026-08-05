// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain
//  Layer:          Tests (Unit)
//  File:           DischargeLimitAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the DischargeLimit aggregate root.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Exceptions;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain;

public sealed class DischargeLimitAggregateTests : UnitTestBase
{
    private static Agg DefinedLimit() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        siteId: Guid.NewGuid(),
        dischargePointId: Guid.NewGuid(),
        dischargePermitId: Guid.NewGuid(),
        parameter: DischargeParameterType.PH,
        limitBasis: LimitBasisType.DailyMaximum,
        limitValue: 9.0m,
        unitOfMeasure: "pH",
        effectiveAtUtc: FixedTimestamp,
        permitConditionReference: "Part I.A.1");

    [Fact]
    public void Create_WithValidData_DefinesActiveLimitAndRaisesEvent()
    {
        var limit = DefinedLimit();

        limit.Status.Should().Be(DischargeLimitStatus.Active);
        limit.LimitValue.Should().Be(9.0m);
        limit.RevisionCount.Should().Be(0);
        limit.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Revise_WhenActive_UpdatesValueAndIncrementsRevisionCount()
    {
        var limit = DefinedLimit();

        limit.Revise(8.5m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        limit.LimitValue.Should().Be(8.5m);
        limit.RevisionCount.Should().Be(1);
    }

    [Fact]
    public void Revise_Twice_AccumulatesRevisionCount()
    {
        var limit = DefinedLimit();

        limit.Revise(8.5m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        limit.Revise(8.0m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        limit.RevisionCount.Should().Be(2);
        limit.LimitValue.Should().Be(8.0m);
    }

    [Fact]
    public void Retire_WhenActive_TransitionsToRetired()
    {
        var limit = DefinedLimit();

        limit.Retire("Superseded by revised permit condition.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        limit.Status.Should().Be(DischargeLimitStatus.Retired);
    }

    [Fact]
    public void Revise_WhenRetired_ThrowsInvalidStatusTransition()
    {
        var limit = DefinedLimit();
        limit.Retire(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => limit.Revise(7.0m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargeLimitInvalidStatusTransitionException>();
    }

    [Fact]
    public void Retire_WhenAlreadyRetired_ThrowsInvalidStatusTransition()
    {
        var limit = DefinedLimit();
        limit.Retire(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => limit.Retire(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargeLimitInvalidStatusTransitionException>();
    }
}
