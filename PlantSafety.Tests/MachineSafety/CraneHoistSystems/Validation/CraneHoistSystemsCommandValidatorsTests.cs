// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Validation
//  Layer:          Tests (Unit)
//  File:           CraneHoistSystemsCommandValidatorsTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FluentValidation validators of all 8 CraneHoistSystems
//                  commands (Register, RecordLiftEvent, RecordOverloadEvent,
//                  ScheduleCraneInspection, CompleteCraneInspection, QualifyOperator,
//                  SuspendOperatorQualification, RecordRiggingInspection).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Application.MachineSafety.Validation;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Validation;

public sealed class CraneHoistSystemsCommandValidatorsTests : UnitTestBase
{
    // ---- RegisterCraneHoistAssetCommand ---------------------------------------

    private static RegisterCraneHoistAssetCommand ValidRegisterCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, "CRANE-1", "Crane One", "OverheadBridgeCrane", 5000m, TestUserId.Value);

    [Fact]
    public void RegisterValidator_WithValidCommand_PassesValidation()
    {
        new RegisterCraneHoistAssetCommandValidator().TestValidate(ValidRegisterCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterValidator_WithEmptyAssetTag_FailsValidation()
    {
        var command = ValidRegisterCommand() with { AssetTag = string.Empty };
        new RegisterCraneHoistAssetCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.AssetTag);
    }

    [Fact]
    public void RegisterValidator_WithZeroRatedLoadCapacity_FailsValidation()
    {
        var command = ValidRegisterCommand() with { RatedLoadCapacityKg = 0m };
        new RegisterCraneHoistAssetCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.RatedLoadCapacityKg);
    }

    [Fact]
    public void RegisterValidator_WithEmptyTenantId_FailsValidation()
    {
        var command = ValidRegisterCommand() with { TenantId = Guid.Empty };
        new RegisterCraneHoistAssetCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.TenantId);
    }

    // ---- RecordLiftEventCommand -----------------------------------------------

    private static RecordLiftEventCommand ValidLiftCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), 1200m, TestUserId.Value);

    [Fact]
    public void RecordLiftEventValidator_WithValidCommand_PassesValidation()
    {
        new RecordLiftEventCommandValidator().TestValidate(ValidLiftCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordLiftEventValidator_WithNonPositiveLoad_FailsValidation()
    {
        var command = ValidLiftCommand() with { ObservedLoadKg = 0m };
        new RecordLiftEventCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.ObservedLoadKg);
    }

    [Fact]
    public void RecordLiftEventValidator_WithEmptyCraneHoistSystemsId_FailsValidation()
    {
        var command = ValidLiftCommand() with { CraneHoistSystemsId = Guid.Empty };
        new RecordLiftEventCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.CraneHoistSystemsId);
    }

    // ---- RecordOverloadEventCommand ---------------------------------------------

    private static RecordOverloadEventCommand ValidOverloadCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), 6500m, TestUserId.Value);

    [Fact]
    public void RecordOverloadEventValidator_WithValidCommand_PassesValidation()
    {
        new RecordOverloadEventCommandValidator().TestValidate(ValidOverloadCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordOverloadEventValidator_WithNonPositiveLoad_FailsValidation()
    {
        var command = ValidOverloadCommand() with { ObservedLoadKg = -1m };
        new RecordOverloadEventCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.ObservedLoadKg);
    }

    // ---- ScheduleCraneInspectionCommand -----------------------------------------

    private static ScheduleCraneInspectionCommand ValidScheduleCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), FixedUtcNow.AddDays(30), TestUserId.Value);

    [Fact]
    public void ScheduleCraneInspectionValidator_WithValidCommand_PassesValidation()
    {
        new ScheduleCraneInspectionCommandValidator().TestValidate(ValidScheduleCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ScheduleCraneInspectionValidator_WithDefaultDueDate_FailsValidation()
    {
        var command = ValidScheduleCommand() with { DueAtUtc = default };
        new ScheduleCraneInspectionCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.DueAtUtc);
    }

    // ---- CompleteCraneInspectionCommand -----------------------------------------

    private static CompleteCraneInspectionCommand ValidCompleteCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), true, TestUserId.Value);

    [Fact]
    public void CompleteCraneInspectionValidator_WithValidCommand_PassesValidation()
    {
        new CompleteCraneInspectionCommandValidator().TestValidate(ValidCompleteCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CompleteCraneInspectionValidator_WithEmptyPerformedByUserId_FailsValidation()
    {
        var command = ValidCompleteCommand() with { PerformedByUserId = Guid.Empty };
        new CompleteCraneInspectionCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }

    // ---- QualifyOperatorCommand ------------------------------------------------

    private static QualifyOperatorCommand ValidQualifyCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow.AddYears(1), TestUserId.Value);

    [Fact]
    public void QualifyOperatorValidator_WithValidCommand_PassesValidation()
    {
        new QualifyOperatorCommandValidator().TestValidate(ValidQualifyCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void QualifyOperatorValidator_WithEmptyOperatorUserId_FailsValidation()
    {
        var command = ValidQualifyCommand() with { OperatorUserId = Guid.Empty };
        new QualifyOperatorCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.OperatorUserId);
    }

    [Fact]
    public void QualifyOperatorValidator_WithNullExpiry_PassesValidation()
    {
        var command = ValidQualifyCommand() with { ExpiresAtUtc = null };
        new QualifyOperatorCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ---- SuspendOperatorQualificationCommand ------------------------------------

    private static SuspendOperatorQualificationCommand ValidSuspendCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "failed re-certification", TestUserId.Value);

    [Fact]
    public void SuspendOperatorQualificationValidator_WithValidCommand_PassesValidation()
    {
        new SuspendOperatorQualificationCommandValidator().TestValidate(ValidSuspendCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SuspendOperatorQualificationValidator_WithEmptyReason_FailsValidation()
    {
        var command = ValidSuspendCommand() with { Reason = string.Empty };
        new SuspendOperatorQualificationCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.Reason);
    }

    [Fact]
    public void SuspendOperatorQualificationValidator_WithReasonExceedingMaxLength_FailsValidation()
    {
        var command = ValidSuspendCommand() with { Reason = new string('x', 2001) };
        new SuspendOperatorQualificationCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.Reason);
    }

    // ---- RecordRiggingInspectionCommand ------------------------------------------

    private static RecordRiggingInspectionCommand ValidRiggingCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), true, "frayed sling", TestUserId.Value);

    [Fact]
    public void RecordRiggingInspectionValidator_WithValidCommand_PassesValidation()
    {
        new RecordRiggingInspectionCommandValidator().TestValidate(ValidRiggingCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordRiggingInspectionValidator_WithNullNotes_PassesValidation()
    {
        var command = ValidRiggingCommand() with { Notes = null };
        new RecordRiggingInspectionCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordRiggingInspectionValidator_WithNotesExceedingMaxLength_FailsValidation()
    {
        var command = ValidRiggingCommand() with { Notes = new string('n', 2001) };
        new RecordRiggingInspectionCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(c => c.Notes);
    }
}
