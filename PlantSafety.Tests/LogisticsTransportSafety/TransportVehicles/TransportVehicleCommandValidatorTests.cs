// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportVehicles
//  Layer:          Tests (Unit)
//  File:           TransportVehicleCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FluentValidation validators backing all TransportVehicle
//                  commands: Register, ScheduleInspection, RecordInspectionResult, Retire.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.Validation;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportVehicles;

public sealed class TransportVehicleCommandValidatorTests : UnitTestBase
{
    // ---- RegisterTransportVehicleCommandValidator -----------------------------------

    private static RegisterTransportVehicleCommand ValidRegisterCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "Forklift 12", "Forklift", "FLT-012", TestUserId.Value);

    [Fact]
    public void Register_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RegisterTransportVehicleCommandValidator().TestValidate(ValidRegisterCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Register_WithEmptyVehicleTitle_ShouldHaveError()
    {
        var command = ValidRegisterCommand() with { VehicleTitle = "" };
        var result = new RegisterTransportVehicleCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleTitle);
    }

    [Fact]
    public void Register_WithTooLongVehicleTitle_ShouldHaveError()
    {
        var command = ValidRegisterCommand() with { VehicleTitle = new string('x', 201) };
        var result = new RegisterTransportVehicleCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleTitle);
    }

    [Fact]
    public void Register_WithEmptyVehicleIdentifier_ShouldHaveError()
    {
        var command = ValidRegisterCommand() with { VehicleIdentifier = "" };
        var result = new RegisterTransportVehicleCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleIdentifier);
    }

    [Fact]
    public void Register_WithEmptyTenantId_ShouldHaveError()
    {
        var command = ValidRegisterCommand() with { TenantId = Guid.Empty };
        var result = new RegisterTransportVehicleCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TenantId);
    }

    // ---- ScheduleInspectionTransportVehicleCommandValidator --------------------------

    private static ScheduleInspectionTransportVehicleCommand ValidScheduleCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), FixedUtcNow.AddDays(7), TestUserId.Value);

    [Fact]
    public void ScheduleInspection_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new ScheduleInspectionTransportVehicleCommandValidator().TestValidate(ValidScheduleCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ScheduleInspection_WithEmptyVehicleId_ShouldHaveError()
    {
        var command = ValidScheduleCommand() with { TransportVehicleId = Guid.Empty };
        var result = new ScheduleInspectionTransportVehicleCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TransportVehicleId);
    }

    // ---- RecordInspectionResultTransportVehicleCommandValidator -----------------------

    private static RecordInspectionResultTransportVehicleCommand ValidRecordCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), true, 90m, TestUserId.Value);

    [Fact]
    public void RecordInspectionResult_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RecordInspectionResultTransportVehicleCommandValidator().TestValidate(ValidRecordCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void RecordInspectionResult_WithScoreOutOfRange_ShouldHaveError(double score)
    {
        var command = ValidRecordCommand() with { InspectionScore = (decimal)score };
        var result = new RecordInspectionResultTransportVehicleCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.InspectionScore);
    }

    // ---- RetireTransportVehicleCommandValidator ---------------------------------------

    private static RetireTransportVehicleCommand ValidRetireCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void Retire_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RetireTransportVehicleCommandValidator().TestValidate(ValidRetireCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Retire_WithEmptyPerformedByUserId_ShouldHaveError()
    {
        var command = ValidRetireCommand() with { PerformedByUserId = Guid.Empty };
        var result = new RetireTransportVehicleCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }
}
