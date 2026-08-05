// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent.Validation
//  Layer:          Tests (Unit)
//  File:           RecordHumanRobotInteractionEventCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordHumanRobotInteractionEventCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.Validation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent.Validation;

public sealed class RecordHumanRobotInteractionEventCommandValidatorTests : UnitTestBase
{
    private readonly RecordHumanRobotInteractionEventCommandValidator _validator = new();

    private static RecordHumanRobotInteractionEventCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, Guid.NewGuid(), "NearMiss", "High", 0.4m, "Loading dock 3", TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyAutonomousVehicleAssetId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { AutonomousVehicleAssetId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.AutonomousVehicleAssetId);
    }

    [Fact]
    public void EmptyInteractionType_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { InteractionType = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.InteractionType);
    }

    [Fact]
    public void NegativeDistanceMeters_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { DistanceMeters = -1m });
        result.ShouldHaveValidationErrorFor(c => c.DistanceMeters);
    }

    [Fact]
    public void NullDistanceMeters_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { DistanceMeters = null });
        result.ShouldNotHaveValidationErrorFor(c => c.DistanceMeters);
    }
}
