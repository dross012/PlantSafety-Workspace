// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Tests (Unit)
//  File:           LoadSecurementCheckCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FluentValidation validators backing all
//                  LoadSecurementCheck commands: Record, CompleteCorrectiveAction.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.Validation;

namespace PlantSafety.Tests.LogisticsTransportSafety.LoadSecurementChecks;

public sealed class LoadSecurementCheckCommandValidatorTests : UnitTestBase
{
    // ---- RecordLoadSecurementCheckCommandValidator ------------------------------------

    private static RecordLoadSecurementCheckCommand ValidRecordCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
        Guid.NewGuid(), Guid.NewGuid(), true, 0, 100m, TestUserId.Value);

    [Fact]
    public void Record_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RecordLoadSecurementCheckCommandValidator().TestValidate(ValidRecordCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Record_WithEmptyVehicleId_ShouldHaveError()
    {
        var command = ValidRecordCommand() with { VehicleId = Guid.Empty };
        var result = new RecordLoadSecurementCheckCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Record_WithNegativeDeficiencyCount_ShouldHaveError()
    {
        var command = ValidRecordCommand() with { DeficiencyCount = -1 };
        var result = new RecordLoadSecurementCheckCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DeficiencyCount);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Record_WithScoreOutOfRange_ShouldHaveError(double score)
    {
        var command = ValidRecordCommand() with { SecurementScore = (decimal)score };
        var result = new RecordLoadSecurementCheckCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SecurementScore);
    }

    // ---- CompleteCorrectiveActionLoadSecurementCheckCommandValidator ------------------

    private static CompleteCorrectiveActionLoadSecurementCheckCommand ValidCompleteCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void CompleteCorrectiveAction_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new CompleteCorrectiveActionLoadSecurementCheckCommandValidator().TestValidate(ValidCompleteCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CompleteCorrectiveAction_WithEmptyLoadSecurementCheckId_ShouldHaveError()
    {
        var command = ValidCompleteCommand() with { LoadSecurementCheckId = Guid.Empty };
        var result = new CompleteCorrectiveActionLoadSecurementCheckCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LoadSecurementCheckId);
    }
}
