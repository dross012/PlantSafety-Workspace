// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityNotification.Validation
//  Layer:          Tests (Unit)
//  File:           SendCommunityNotificationCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SendCommunityNotificationCommandValidator rules.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.Validation;

namespace PlantSafety.Tests.CommunitySafety.CommunityNotification.Validation;

public sealed class SendCommunityNotificationCommandValidatorTests : UnitTestBase
{
    private readonly SendCommunityNotificationCommandValidator _validator = new();

    private static SendCommunityNotificationCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
        "Email", Guid.NewGuid(), "Planned maintenance notice", "Elevated flare activity expected Tuesday 2-4am.",
        null, null, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyRecipientContactId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { RecipientContactId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.RecipientContactId);
    }

    [Fact]
    public void EmptySubject_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Subject = "" });
        result.ShouldHaveValidationErrorFor(c => c.Subject);
    }

    [Fact]
    public void SubjectTooLong_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Subject = new string('x', 301) });
        result.ShouldHaveValidationErrorFor(c => c.Subject);
    }

    [Fact]
    public void EmptyMessageBody_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { MessageBody = "" });
        result.ShouldHaveValidationErrorFor(c => c.MessageBody);
    }

    [Fact]
    public void EmptyChannel_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Channel = "" });
        result.ShouldHaveValidationErrorFor(c => c.Channel);
    }
}
