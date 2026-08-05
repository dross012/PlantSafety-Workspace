// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportTrips
//  Layer:          Tests (Unit)
//  File:           TransportTripCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FluentValidation validators backing all TransportTrip
//                  commands: Plan, Complete, Cancel.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.Validation;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportTrips;

public sealed class TransportTripCommandValidatorTests : UnitTestBase
{
    // ---- PlanTransportTripCommandValidator --------------------------------------------

    private static PlanTransportTripCommand ValidPlanCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        Guid.NewGuid(), Guid.NewGuid(), "OffSiteDelivery", "Dock 3 to Warehouse B",
        FixedUtcNow.AddHours(1), FixedUtcNow.AddHours(3), TestUserId.Value);

    [Fact]
    public void Plan_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new PlanTransportTripCommandValidator().TestValidate(ValidPlanCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Plan_WithEmptyVehicleId_ShouldHaveError()
    {
        var command = ValidPlanCommand() with { VehicleId = Guid.Empty };
        var result = new PlanTransportTripCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Plan_WithEmptyRouteDescription_ShouldHaveError()
    {
        var command = ValidPlanCommand() with { RouteDescription = "" };
        var result = new PlanTransportTripCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.RouteDescription);
    }

    [Fact]
    public void Plan_WithArrivalNotAfterDeparture_ShouldHaveError()
    {
        var command = ValidPlanCommand() with { PlannedDepartureAtUtc = FixedUtcNow, PlannedArrivalAtUtc = FixedUtcNow };
        var result = new PlanTransportTripCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PlannedArrivalAtUtc);
    }

    // ---- CompleteTransportTripCommandValidator ----------------------------------------

    private static CompleteTransportTripCommand ValidCompleteCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), FixedUtcNow, FixedUtcNow.AddHours(2), 42.5m, TestUserId.Value);

    [Fact]
    public void Complete_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new CompleteTransportTripCommandValidator().TestValidate(ValidCompleteCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Complete_WithArrivalBeforeDeparture_ShouldHaveError()
    {
        var command = ValidCompleteCommand() with { ActualArrivalAtUtc = FixedUtcNow.AddHours(-1) };
        var result = new CompleteTransportTripCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ActualArrivalAtUtc);
    }

    [Fact]
    public void Complete_WithNegativeDistance_ShouldHaveError()
    {
        var command = ValidCompleteCommand() with { DistanceMiles = -1m };
        var result = new CompleteTransportTripCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DistanceMiles);
    }

    // ---- CancelTransportTripCommandValidator ------------------------------------------

    private static CancelTransportTripCommand ValidCancelCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void Cancel_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new CancelTransportTripCommandValidator().TestValidate(ValidCancelCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Cancel_WithEmptyTransportTripId_ShouldHaveError()
    {
        var command = ValidCancelCommand() with { TransportTripId = Guid.Empty };
        var result = new CancelTransportTripCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TransportTripId);
    }
}
