// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Validation
//  Layer:          Tests (Unit)
//  File:           BroadcastPublicAlertCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Validator tests for BroadcastPublicAlertCommandValidator. Isolated,
//                  deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Application.PublicSafetyCommunication.Validation;

namespace PlantSafety.Tests.PublicSafetyCommunication.Validation;

public sealed class BroadcastPublicAlertCommandValidatorTests : UnitTestBase
{
    private readonly BroadcastPublicAlertCommandValidator _validator = new();

    private static BroadcastPublicAlertCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: TestTenantId.Value,
        SiteId: null,
        IncidentId: Guid.NewGuid(),
        SeverityCode: "Warning",
        ChannelCode: "Ipaws",
        Headline: "Shelter in place",
        Message: "A chemical release has occurred. Residents should shelter in place.",
        TargetArea: "1-mile radius");

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptySeverityCode_HasError() =>
        _validator.TestValidate(ValidCommand() with { SeverityCode = "" })
            .ShouldHaveValidationErrorFor(x => x.SeverityCode);

    [Fact]
    public void Validate_WithEmptyChannelCode_HasError() =>
        _validator.TestValidate(ValidCommand() with { ChannelCode = "" })
            .ShouldHaveValidationErrorFor(x => x.ChannelCode);

    [Fact]
    public void Validate_WithHeadlineTooLong_HasError() =>
        _validator.TestValidate(ValidCommand() with { Headline = new string('H', 201) })
            .ShouldHaveValidationErrorFor(x => x.Headline);

    [Fact]
    public void Validate_WithEmptyTargetArea_HasError() =>
        _validator.TestValidate(ValidCommand() with { TargetArea = "" })
            .ShouldHaveValidationErrorFor(x => x.TargetArea);
}
