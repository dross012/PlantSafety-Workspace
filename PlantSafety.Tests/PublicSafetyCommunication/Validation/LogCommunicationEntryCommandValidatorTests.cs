// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Validation
//  Layer:          Tests (Unit)
//  File:           LogCommunicationEntryCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Validator tests for LogCommunicationEntryCommandValidator. Isolated,
//                  deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Application.PublicSafetyCommunication.Validation;

namespace PlantSafety.Tests.PublicSafetyCommunication.Validation;

public sealed class LogCommunicationEntryCommandValidatorTests : UnitTestBase
{
    private readonly LogCommunicationEntryCommandValidator _validator = new();

    private static LogCommunicationEntryCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: TestTenantId.Value,
        SiteId: null,
        IncidentId: Guid.NewGuid(),
        RelatedIncidentNotificationId: null,
        RelatedPublicAlertBroadcastId: null,
        Direction: "Outbound",
        Medium: "Phone",
        CounterpartyName: "County Fire Dispatch",
        Summary: "Notified county fire dispatch of the release.",
        OccurredAtUtc: DateTime.UtcNow);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors() =>
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyDirection_HasError() =>
        _validator.TestValidate(ValidCommand() with { Direction = "" })
            .ShouldHaveValidationErrorFor(x => x.Direction);

    [Fact]
    public void Validate_WithEmptyMedium_HasError() =>
        _validator.TestValidate(ValidCommand() with { Medium = "" })
            .ShouldHaveValidationErrorFor(x => x.Medium);

    [Fact]
    public void Validate_WithEmptyCounterpartyName_HasError() =>
        _validator.TestValidate(ValidCommand() with { CounterpartyName = "" })
            .ShouldHaveValidationErrorFor(x => x.CounterpartyName);

    [Fact]
    public void Validate_WithEmptySummary_HasError() =>
        _validator.TestValidate(ValidCommand() with { Summary = "" })
            .ShouldHaveValidationErrorFor(x => x.Summary);

    [Fact]
    public void Validate_WithDefaultOccurredAtUtc_HasError() =>
        _validator.TestValidate(ValidCommand() with { OccurredAtUtc = default })
            .ShouldHaveValidationErrorFor(x => x.OccurredAtUtc);
}
