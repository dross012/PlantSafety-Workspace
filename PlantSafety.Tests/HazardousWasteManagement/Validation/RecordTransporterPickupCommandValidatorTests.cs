// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Validation
//  Layer:          Tests (Unit)
//  File:           RecordTransporterPickupCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Unit tests for RecordTransporterPickupCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Validation;

namespace PlantSafety.Tests.HazardousWasteManagement.Validation;

public sealed class RecordTransporterPickupCommandValidatorTests : UnitTestBase
{
    private readonly RecordTransporterPickupCommandValidator _validator = new();

    private static RecordTransporterPickupCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyHazardousWasteManifestId_ShouldFailValidation()
    {
        var command = ValidCommand() with { HazardousWasteManifestId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.HazardousWasteManifestId);
    }

    [Fact]
    public void EmptyTransporterId_ShouldFailValidation()
    {
        var command = ValidCommand() with { TransporterId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TransporterId);
    }
}
