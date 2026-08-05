// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Validation
//  Layer:          Tests (Unit)
//  File:           IncidentNotificationValidatorsTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Validator tests for DispatchIncidentNotificationCommandValidator,
//                  AcknowledgeIncidentNotificationCommandValidator, and
//                  CloseIncidentNotificationCommandValidator. Isolated, deterministic, <50ms per
//                  test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Application.PublicSafetyCommunication.Validation;

namespace PlantSafety.Tests.PublicSafetyCommunication.Validation;

public sealed class DispatchIncidentNotificationCommandValidatorTests : UnitTestBase
{
    private readonly DispatchIncidentNotificationCommandValidator _validator = new();

    private static DispatchIncidentNotificationCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: TestTenantId.Value,
        SiteId: null,
        IncidentId: Guid.NewGuid(),
        PublicSafetyAgencyId: Guid.NewGuid(),
        DeliveryChannelCode: "Phone",
        Message: "Ammonia release -- HazMat requested.");

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyPublicSafetyAgencyId_HasError() =>
        _validator.TestValidate(ValidCommand() with { PublicSafetyAgencyId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.PublicSafetyAgencyId);

    [Fact]
    public void Validate_WithEmptyMessage_HasError() =>
        _validator.TestValidate(ValidCommand() with { Message = "" })
            .ShouldHaveValidationErrorFor(x => x.Message);

    [Fact]
    public void Validate_WithMessageTooLong_HasError() =>
        _validator.TestValidate(ValidCommand() with { Message = new string('A', 4001) })
            .ShouldHaveValidationErrorFor(x => x.Message);
}

public sealed class AcknowledgeIncidentNotificationCommandValidatorTests : UnitTestBase
{
    private readonly AcknowledgeIncidentNotificationCommandValidator _validator = new();

    private static AcknowledgeIncidentNotificationCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: TestTenantId.Value,
        IncidentNotificationId: Guid.NewGuid(),
        AcknowledgedByContactName: "County Dispatch Desk 4");

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyIncidentNotificationId_HasError() =>
        _validator.TestValidate(ValidCommand() with { IncidentNotificationId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.IncidentNotificationId);

    [Fact]
    public void Validate_WithEmptyAcknowledgedByContactName_HasError() =>
        _validator.TestValidate(ValidCommand() with { AcknowledgedByContactName = "" })
            .ShouldHaveValidationErrorFor(x => x.AcknowledgedByContactName);
}

public sealed class CloseIncidentNotificationCommandValidatorTests : UnitTestBase
{
    private readonly CloseIncidentNotificationCommandValidator _validator = new();

    private static CloseIncidentNotificationCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: TestTenantId.Value,
        IncidentNotificationId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyIncidentNotificationId_HasError() =>
        _validator.TestValidate(ValidCommand() with { IncidentNotificationId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.IncidentNotificationId);

    [Fact]
    public void Validate_WithEmptyTenantId_HasError() =>
        _validator.TestValidate(ValidCommand() with { TenantId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.TenantId);
}
