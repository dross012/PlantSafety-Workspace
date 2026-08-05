// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Validation
//  Layer:          Tests (Unit)
//  File:           OpenAccumulationContainerCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Unit tests for OpenAccumulationContainerCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Validation;

namespace PlantSafety.Tests.HazardousWasteManagement.Validation;

public sealed class OpenAccumulationContainerCommandValidatorTests : UnitTestBase
{
    private readonly OpenAccumulationContainerCommandValidator _validator = new();

    private static OpenAccumulationContainerCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        Guid.NewGuid(), Guid.NewGuid(), "DRUM-001", 55m, 90, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyAccumulationAreaId_ShouldFailValidation()
    {
        var command = ValidCommand() with { AccumulationAreaId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AccumulationAreaId);
    }

    [Fact]
    public void EmptyWasteStreamId_ShouldFailValidation()
    {
        var command = ValidCommand() with { WasteStreamId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.WasteStreamId);
    }

    [Fact]
    public void EmptyContainerLabel_ShouldFailValidation()
    {
        var command = ValidCommand() with { ContainerLabel = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ContainerLabel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void NonPositiveCapacityGallons_ShouldFailValidation(decimal capacity)
    {
        var command = ValidCommand() with { CapacityGallons = capacity };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CapacityGallons);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveRegulatoryTimeLimitDays_ShouldFailValidation(int days)
    {
        var command = ValidCommand() with { RegulatoryTimeLimitDays = days };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.RegulatoryTimeLimitDays);
    }
}
