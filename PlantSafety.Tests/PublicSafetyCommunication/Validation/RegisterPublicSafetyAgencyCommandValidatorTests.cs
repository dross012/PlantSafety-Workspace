// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Validation
//  Layer:          Tests (Unit)
//  File:           RegisterPublicSafetyAgencyCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Validator tests for RegisterPublicSafetyAgencyCommandValidator. Isolated,
//                  deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Application.PublicSafetyCommunication.Validation;

namespace PlantSafety.Tests.PublicSafetyCommunication.Validation;

public sealed class RegisterPublicSafetyAgencyCommandValidatorTests : UnitTestBase
{
    private readonly RegisterPublicSafetyAgencyCommandValidator _validator = new();

    private static RegisterPublicSafetyAgencyCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: TestTenantId.Value,
        SiteId: null,
        AgencyName: "County Fire Department",
        AgencyType: "Fire",
        JurisdictionDescription: "County district 4",
        PrimaryPhoneNumber: "555-0100",
        PrimaryEmail: "dispatch@countyfire.example");

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyAgencyName_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { AgencyName = "" });
        result.ShouldHaveValidationErrorFor(x => x.AgencyName);
    }

    [Fact]
    public void Validate_WithAgencyNameTooLong_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { AgencyName = new string('A', 401) });
        result.ShouldHaveValidationErrorFor(x => x.AgencyName);
    }

    [Fact]
    public void Validate_WithEmptyTenantId_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { TenantId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Validate_WithEmptyPrimaryPhoneNumber_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { PrimaryPhoneNumber = "" });
        result.ShouldHaveValidationErrorFor(x => x.PrimaryPhoneNumber);
    }

    [Fact]
    public void Validate_WithInvalidPrimaryEmail_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { PrimaryEmail = "not-an-email" });
        result.ShouldHaveValidationErrorFor(x => x.PrimaryEmail);
    }

    [Fact]
    public void Validate_WithNullPrimaryEmail_HasNoError()
    {
        var result = _validator.TestValidate(ValidCommand() with { PrimaryEmail = null });
        result.ShouldNotHaveValidationErrorFor(x => x.PrimaryEmail);
    }
}
