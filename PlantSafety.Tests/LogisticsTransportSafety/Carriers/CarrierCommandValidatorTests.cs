// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.Carriers
//  Layer:          Tests (Unit)
//  File:           CarrierCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FluentValidation validators backing all Carrier commands:
//                  Register, Qualify, Suspend, Retire.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.Validation;

namespace PlantSafety.Tests.LogisticsTransportSafety.Carriers;

public sealed class CarrierCommandValidatorTests : UnitTestBase
{
    // ---- RegisterCarrierCommandValidator ---------------------------------------------

    private static RegisterCarrierCommand ValidRegisterCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "Acme Freight Lines", "ThirdPartyContractCarrier", null, TestUserId.Value);

    [Fact]
    public void Register_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RegisterCarrierCommandValidator().TestValidate(ValidRegisterCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Register_WithEmptyCarrierName_ShouldHaveError()
    {
        var command = ValidRegisterCommand() with { CarrierName = "" };
        var result = new RegisterCarrierCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CarrierName);
    }

    [Fact]
    public void Register_WithTooLongCarrierName_ShouldHaveError()
    {
        var command = ValidRegisterCommand() with { CarrierName = new string('x', 201) };
        var result = new RegisterCarrierCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CarrierName);
    }

    [Fact]
    public void Register_WithEmptyCarrierType_ShouldHaveError()
    {
        var command = ValidRegisterCommand() with { CarrierType = "" };
        var result = new RegisterCarrierCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CarrierType);
    }

    // ---- QualifyCarrierCommandValidator -------------------------------------------------

    private static QualifyCarrierCommand ValidQualifyCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), 90m, TestUserId.Value);

    [Fact]
    public void Qualify_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new QualifyCarrierCommandValidator().TestValidate(ValidQualifyCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Qualify_WithScoreOutOfRange_ShouldHaveError(double score)
    {
        var command = ValidQualifyCommand() with { SafetyRatingScore = (decimal)score };
        var result = new QualifyCarrierCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SafetyRatingScore);
    }

    // ---- SuspendCarrierCommandValidator -------------------------------------------------

    private static SuspendCarrierCommand ValidSuspendCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Insurance lapsed", TestUserId.Value);

    [Fact]
    public void Suspend_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new SuspendCarrierCommandValidator().TestValidate(ValidSuspendCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Suspend_WithEmptyReason_ShouldHaveError()
    {
        var command = ValidSuspendCommand() with { Reason = "" };
        var result = new SuspendCarrierCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Reason);
    }

    // ---- RetireCarrierCommandValidator ---------------------------------------------------

    private static RetireCarrierCommand ValidRetireCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void Retire_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RetireCarrierCommandValidator().TestValidate(ValidRetireCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Retire_WithEmptyCarrierId_ShouldHaveError()
    {
        var command = ValidRetireCommand() with { CarrierId = Guid.Empty };
        var result = new RetireCarrierCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CarrierId);
    }
}
