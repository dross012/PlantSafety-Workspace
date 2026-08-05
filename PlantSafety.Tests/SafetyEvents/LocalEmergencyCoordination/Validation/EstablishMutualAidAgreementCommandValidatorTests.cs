// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Validation
//  Layer:          Tests (Unit)
//  File:           EstablishMutualAidAgreementCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation tests for EstablishMutualAidAgreementCommandValidator.
//                  Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Validation;

public sealed class EstablishMutualAidAgreementCommandValidatorTests : UnitTestBase
{
    private readonly EstablishMutualAidAgreementCommandValidator _validator = new();

    private static EstablishMutualAidAgreementCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        LocalEmergencyCoordinationId: Guid.NewGuid(),
        AgencyName: "Metro Fire Dept",
        Discipline: "Fire",
        DocumentVersion: 1,
        EffectiveFromUtc: FixedUtcNow,
        RenewalDueAtUtc: FixedUtcNow.AddYears(1),
        EstablishedByUserId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyLocalEmergencyCoordinationId_HasError()
        => _validator.TestValidate(ValidCommand() with { LocalEmergencyCoordinationId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.LocalEmergencyCoordinationId);

    [Fact]
    public void Validate_WithEmptyAgencyName_HasError()
        => _validator.TestValidate(ValidCommand() with { AgencyName = "" }).ShouldHaveValidationErrorFor(x => x.AgencyName);

    [Fact]
    public void Validate_WithEmptyDiscipline_HasError()
        => _validator.TestValidate(ValidCommand() with { Discipline = "" }).ShouldHaveValidationErrorFor(x => x.Discipline);

    [Fact]
    public void Validate_WithDocumentVersionBelowOne_HasError()
        => _validator.TestValidate(ValidCommand() with { DocumentVersion = 0 }).ShouldHaveValidationErrorFor(x => x.DocumentVersion);

    [Fact]
    public void Validate_WhenRenewalDueBeforeEffectiveFrom_HasError()
        => _validator.TestValidate(ValidCommand() with { RenewalDueAtUtc = FixedUtcNow.AddDays(-1), EffectiveFromUtc = FixedUtcNow })
            .ShouldHaveValidationErrorFor(x => x.RenewalDueAtUtc);

    [Fact]
    public void Validate_WithAgencyNameExceedingMaxLength_HasError()
        => _validator.TestValidate(ValidCommand() with { AgencyName = new string('x', 201) }).ShouldHaveValidationErrorFor(x => x.AgencyName);
}
