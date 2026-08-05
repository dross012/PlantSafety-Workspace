// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.DriverQualifications
//  Layer:          Tests (Unit)
//  File:           DriverQualificationCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FluentValidation validators backing all
//                  DriverQualification commands: Qualify, Suspend, Reinstate, Retire.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.Validation;

namespace PlantSafety.Tests.LogisticsTransportSafety.DriverQualifications;

public sealed class DriverQualificationCommandValidatorTests : UnitTestBase
{
    // ---- QualifyDriverCommandValidator -----------------------------------------------

    private static QualifyDriverCommand ValidQualifyCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        Guid.NewGuid(), "CommercialDriverLicense", "CDL-1", FixedUtcNow.AddYears(-1), FixedUtcNow.AddYears(1), TestUserId.Value);

    [Fact]
    public void Qualify_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new QualifyDriverCommandValidator().TestValidate(ValidQualifyCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Qualify_WithEmptyLicenseNumber_ShouldHaveError()
    {
        var command = ValidQualifyCommand() with { LicenseNumber = "" };
        var result = new QualifyDriverCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LicenseNumber);
    }

    [Fact]
    public void Qualify_WithTooLongLicenseNumber_ShouldHaveError()
    {
        var command = ValidQualifyCommand() with { LicenseNumber = new string('x', 65) };
        var result = new QualifyDriverCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LicenseNumber);
    }

    [Fact]
    public void Qualify_WithExpiresBeforeIssued_ShouldHaveError()
    {
        var command = ValidQualifyCommand() with { IssuedAtUtc = FixedUtcNow, ExpiresAtUtc = FixedUtcNow.AddDays(-1) };
        var result = new QualifyDriverCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ExpiresAtUtc);
    }

    [Fact]
    public void Qualify_WithEmptyDriverUserId_ShouldHaveError()
    {
        var command = ValidQualifyCommand() with { DriverUserId = Guid.Empty };
        var result = new QualifyDriverCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DriverUserId);
    }

    // ---- SuspendDriverQualificationCommandValidator -----------------------------------

    private static SuspendDriverQualificationCommand ValidSuspendCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Failed drug test", TestUserId.Value);

    [Fact]
    public void Suspend_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new SuspendDriverQualificationCommandValidator().TestValidate(ValidSuspendCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Suspend_WithTooLongReason_ShouldHaveError()
    {
        var command = ValidSuspendCommand() with { Reason = new string('x', 1001) };
        var result = new SuspendDriverQualificationCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Reason);
    }

    // ---- ReinstateDriverQualificationCommandValidator ---------------------------------

    private static ReinstateDriverQualificationCommand ValidReinstateCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void Reinstate_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new ReinstateDriverQualificationCommandValidator().TestValidate(ValidReinstateCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Reinstate_WithEmptyDriverQualificationId_ShouldHaveError()
    {
        var command = ValidReinstateCommand() with { DriverQualificationId = Guid.Empty };
        var result = new ReinstateDriverQualificationCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DriverQualificationId);
    }

    // ---- RetireDriverQualificationCommandValidator ------------------------------------

    private static RetireDriverQualificationCommand ValidRetireCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void Retire_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RetireDriverQualificationCommandValidator().TestValidate(ValidRetireCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Retire_WithEmptyPerformedByUserId_ShouldHaveError()
    {
        var command = ValidRetireCommand() with { PerformedByUserId = Guid.Empty };
        var result = new RetireDriverQualificationCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }
}
