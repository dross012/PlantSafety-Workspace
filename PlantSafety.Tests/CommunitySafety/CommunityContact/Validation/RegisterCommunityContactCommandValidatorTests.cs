// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityContact.Validation
//  Layer:          Tests (Unit)
//  File:           RegisterCommunityContactCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterCommunityContactCommandValidator rules.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.Validation;

namespace PlantSafety.Tests.CommunitySafety.CommunityContact.Validation;

public sealed class RegisterCommunityContactCommandValidatorTests : UnitTestBase
{
    private readonly RegisterCommunityContactCommandValidator _validator = new();

    private static RegisterCommunityContactCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "Fire Chief - Station 4", "EmergencyServices", "North Ridge", "chief@example.org",
        "City Fire Dept", "555-0100", true, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTenantId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { TenantId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.TenantId);
    }

    [Fact]
    public void EmptyDisplayName_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { DisplayName = "" });
        result.ShouldHaveValidationErrorFor(c => c.DisplayName);
    }

    [Fact]
    public void DisplayNameTooLong_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { DisplayName = new string('x', 201) });
        result.ShouldHaveValidationErrorFor(c => c.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidEmail_ShouldFailValidation(string email)
    {
        var result = _validator.TestValidate(ValidCommand() with { Email = email });
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void EmptyCommunityArea_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { CommunityArea = "" });
        result.ShouldHaveValidationErrorFor(c => c.CommunityArea);
    }

    [Fact]
    public void EmptyPerformedByUserId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { PerformedByUserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }

    [Fact]
    public void NullOrganizationAndPhone_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Organization = null, Phone = null });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
