// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for SendCommunityNotificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CommunitySafety.Validation;

/// <summary>Validates <see cref="Commands.SendCommunityNotificationCommand"/>.</summary>
public sealed class SendCommunityNotificationCommandValidator : AbstractValidator<Commands.SendCommunityNotificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SendCommunityNotificationCommand"/>.</summary>
    public SendCommunityNotificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Channel).NotEmpty();
        RuleFor(x => x.RecipientContactId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(300);
        RuleFor(x => x.MessageBody).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
