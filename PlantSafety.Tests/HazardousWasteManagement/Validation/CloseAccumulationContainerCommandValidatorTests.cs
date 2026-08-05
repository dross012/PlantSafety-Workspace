// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Validation
//  Layer:          Tests (Unit)
//  File:           CloseAccumulationContainerCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Unit tests for CloseAccumulationContainerCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Validation;

namespace PlantSafety.Tests.HazardousWasteManagement.Validation;

public sealed class CloseAccumulationContainerCommandValidatorTests : UnitTestBase
{
    private readonly CloseAccumulationContainerCommandValidator _validator = new();

    private static CloseAccumulationContainerCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyWasteContainerId_ShouldFailValidation()
    {
        var command = ValidCommand() with { WasteContainerId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.WasteContainerId);
    }

    [Fact]
    public void EmptyTenantId_ShouldFailValidation()
    {
        var command = ValidCommand() with { TenantId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TenantId);
    }

    [Fact]
    public void EmptyPerformedByUserId_ShouldFailValidation()
    {
        var command = ValidCommand() with { PerformedByUserId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }
}
