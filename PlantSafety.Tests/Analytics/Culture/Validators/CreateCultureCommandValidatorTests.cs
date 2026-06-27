// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Validators
//  Layer:          Tests (Unit)
//  File:           CreateCultureCommandValidatorTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CreateCultureCommandValidator rules.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.Analytics.Culture.Commands;
using PlantSafety.Application.Analytics.Culture.Validators;

namespace PlantSafety.Tests.Analytics.Culture.Validators;

public sealed class CreateCultureCommandValidatorTests : UnitTestBase
{
    private readonly CreateCultureCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var cmd = new CreateCultureCommand(TestTenantId, TestUserId, TestCorrelationId, TestCausationId, TestActor);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTenantId_ShouldFailValidation()
    {
        var cmd = new CreateCultureCommand(TenantId.FromGuid(Guid.Empty), TestUserId, TestCorrelationId, TestCausationId, TestActor);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.TenantId);
    }
}
