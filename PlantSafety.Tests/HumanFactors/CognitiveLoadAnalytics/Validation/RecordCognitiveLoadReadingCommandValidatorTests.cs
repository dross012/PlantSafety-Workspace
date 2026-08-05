// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Validation
//  Layer:          Tests (Unit)
//  File:           RecordCognitiveLoadReadingCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper tests for RecordCognitiveLoadReadingCommandValidator.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Validation;

public sealed class RecordCognitiveLoadReadingCommandValidatorTests : UnitTestBase
{
    private readonly RecordCognitiveLoadReadingCommandValidator _validator = new();

    private static RecordCognitiveLoadReadingCommand ValidCommand() =>
        RecordCognitiveLoadReadingCommand.FromDto(
            tenantId: TestTenantId.Value, siteId: null, shiftType: "Day", shiftDateUtc: FixedUtcNow,
            workerUserId: TestUserId.Value, taskComplexityProfileId: null, cognitiveLoadScore: 50m,
            measurementMethod: "NASA-TLX", measuredAtUtc: FixedUtcNow, requesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTenantId_HasErrorForTenantId()
    {
        var command = ValidCommand() with { TenantId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Validate_WithEmptyWorkerUserId_HasErrorForWorkerUserId()
    {
        var command = ValidCommand() with { WorkerUserId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.WorkerUserId);
    }

    [Fact]
    public void Validate_WithUnrecognizedShiftType_HasErrorForShiftType()
    {
        var command = ValidCommand() with { ShiftType = "Midday" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ShiftType);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void Validate_WithCognitiveLoadScoreOutOfRange_HasErrorForCognitiveLoadScore(decimal score)
    {
        var command = ValidCommand() with { CognitiveLoadScore = score };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.CognitiveLoadScore);
    }

    [Fact]
    public void Validate_WithEmptyMeasurementMethod_HasErrorForMeasurementMethod()
    {
        var command = ValidCommand() with { MeasurementMethod = "" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.MeasurementMethod);
    }

    [Fact]
    public void Validate_WithFutureMeasuredAtUtc_HasErrorForMeasuredAtUtc()
    {
        var command = ValidCommand() with { MeasuredAtUtc = DateTime.UtcNow.AddHours(1) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.MeasuredAtUtc);
    }
}
