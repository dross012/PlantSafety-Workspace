// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for BroadcastPublicAlertCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PublicSafetyCommunication.Validation;

/// <summary>Validates <see cref="Commands.BroadcastPublicAlertCommand"/>.</summary>
public sealed class BroadcastPublicAlertCommandValidator : AbstractValidator<Commands.BroadcastPublicAlertCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BroadcastPublicAlertCommand"/>.</summary>
    public BroadcastPublicAlertCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SeverityCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ChannelCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Headline).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.TargetArea).NotEmpty().MaximumLength(400);
    }
}
