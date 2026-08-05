// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Validation
//  Layer:          Tests (Unit)
//  File:           RecordDisposalCertificateCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Unit tests for RecordDisposalCertificateCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Validation;

namespace PlantSafety.Tests.HazardousWasteManagement.Validation;

public sealed class RecordDisposalCertificateCommandValidatorTests : UnitTestBase
{
    private readonly RecordDisposalCertificateCommandValidator _validator = new();

    private static RecordDisposalCertificateCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        Guid.NewGuid(), Guid.NewGuid(), "Incineration", FixedUtcNow, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyManifestId_ShouldFailValidation()
    {
        var command = ValidCommand() with { ManifestId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManifestId);
    }

    [Fact]
    public void EmptyTsdfFacilityId_ShouldFailValidation()
    {
        var command = ValidCommand() with { TsdfFacilityId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TsdfFacilityId);
    }

    [Fact]
    public void EmptyDisposalMethod_ShouldFailValidation()
    {
        var command = ValidCommand() with { DisposalMethod = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DisposalMethod);
    }

    [Fact]
    public void DefaultDisposalDateUtc_ShouldFailValidation()
    {
        var command = ValidCommand() with { DisposalDateUtc = default };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DisposalDateUtc);
    }
}
