// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Validation
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalSecurityValidatorsTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation unit tests for the five CyberPhysicalSecurity command
//                  validators (one per entity's write command). Grouped together to keep test
//                  volume proportionate to this module's small per-command rule sets.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Validation;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Validation;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Validation;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Validation;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Validation;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Validation;

public sealed class RegisterOtAssetCommandValidatorTests : UnitTestBase
{
    private readonly RegisterOtAssetCommandValidator _validator = new();

    private static RegisterOtAssetCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
        "PLC-101", "Line 3 PLC", "Plc", "Siemens", "S7-1500", "V2.9", "Level 1", "Medium", TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyAssetTag_HasError() =>
        _validator.TestValidate(ValidCommand() with { AssetTag = "" }).ShouldHaveValidationErrorFor(x => x.AssetTag);

    [Fact]
    public void Validate_WithAssetTagOverMaxLength_HasError() =>
        _validator.TestValidate(ValidCommand() with { AssetTag = new string('A', 129) }).ShouldHaveValidationErrorFor(x => x.AssetTag);

    [Fact]
    public void Validate_WithEmptyAssetName_HasError() =>
        _validator.TestValidate(ValidCommand() with { AssetName = "" }).ShouldHaveValidationErrorFor(x => x.AssetName);

    [Fact]
    public void Validate_WithEmptyTenantId_HasError() =>
        _validator.TestValidate(ValidCommand() with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Validate_WithEmptyActorId_HasError() =>
        _validator.TestValidate(ValidCommand() with { ActorId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.ActorId);
}

public sealed class RecordControlChangeEventCommandValidatorTests : UnitTestBase
{
    private readonly RecordControlChangeEventCommandValidator _validator = new();

    private static RecordControlChangeEventCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), Guid.NewGuid(),
        "Setpoint drift", "SetPoint.Temperature", "72.0", "95.0", FixedUtcNow, null, false, TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyChangedParameter_HasError() =>
        _validator.TestValidate(ValidCommand() with { ChangedParameter = "" }).ShouldHaveValidationErrorFor(x => x.ChangedParameter);

    [Fact]
    public void Validate_WithEmptyOtAssetId_HasError() =>
        _validator.TestValidate(ValidCommand() with { OtAssetId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.OtAssetId);

    [Fact]
    public void Validate_WithEmptySiteId_HasError() =>
        _validator.TestValidate(ValidCommand() with { SiteId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.SiteId);
}

public sealed class RunSensorIntegrityCheckCommandValidatorTests : UnitTestBase
{
    private readonly RunSensorIntegrityCheckCommandValidator _validator = new();

    private static RunSensorIntegrityCheckCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), Guid.NewGuid(),
        "Calibration", "Pass", "Within tolerance", TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyCheckType_HasError() =>
        _validator.TestValidate(ValidCommand() with { CheckType = "" }).ShouldHaveValidationErrorFor(x => x.CheckType);

    [Fact]
    public void Validate_WithEmptyResult_HasError() =>
        _validator.TestValidate(ValidCommand() with { Result = "" }).ShouldHaveValidationErrorFor(x => x.Result);
}

public sealed class RaiseSabotageIndicatorCommandValidatorTests : UnitTestBase
{
    private readonly RaiseSabotageIndicatorCommandValidator _validator = new();

    private static RaiseSabotageIndicatorCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
        "ControlChange", "Unauthorized change detected", "High", null, TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyDescription_HasError() =>
        _validator.TestValidate(ValidCommand() with { Description = "" }).ShouldHaveValidationErrorFor(x => x.Description);

    [Fact]
    public void Validate_WithDescriptionOverMaxLength_HasError() =>
        _validator.TestValidate(ValidCommand() with { Description = new string('A', 2001) }).ShouldHaveValidationErrorFor(x => x.Description);

    [Fact]
    public void Validate_WithEmptySource_HasError() =>
        _validator.TestValidate(ValidCommand() with { Source = "" }).ShouldHaveValidationErrorFor(x => x.Source);

    [Fact]
    public void Validate_WithEmptySeverity_HasError() =>
        _validator.TestValidate(ValidCommand() with { Severity = "" }).ShouldHaveValidationErrorFor(x => x.Severity);
}

public sealed class OpenCyberPhysicalIncidentCommandValidatorTests : UnitTestBase
{
    private readonly OpenCyberPhysicalIncidentCommandValidator _validator = new();

    private static OpenCyberPhysicalIncidentCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
        "Coordinated PLC tampering", "description", "High", TestUserId.Value, null, TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyTitle_HasError() =>
        _validator.TestValidate(ValidCommand() with { Title = "" }).ShouldHaveValidationErrorFor(x => x.Title);

    [Fact]
    public void Validate_WithTitleOverMaxLength_HasError() =>
        _validator.TestValidate(ValidCommand() with { Title = new string('A', 257) }).ShouldHaveValidationErrorFor(x => x.Title);

    [Fact]
    public void Validate_WithEmptyOwnerId_HasError() =>
        _validator.TestValidate(ValidCommand() with { OwnerId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.OwnerId);

    [Fact]
    public void Validate_WithEmptySeverity_HasError() =>
        _validator.TestValidate(ValidCommand() with { Severity = "" }).ShouldHaveValidationErrorFor(x => x.Severity);
}
