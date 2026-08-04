// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for CloseIncidentNotificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PublicSafetyCommunication.Validation;

/// <summary>Validates <see cref="Commands.CloseIncidentNotificationCommand"/>.</summary>
public sealed class CloseIncidentNotificationCommandValidator : AbstractValidator<Commands.CloseIncidentNotificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CloseIncidentNotificationCommand"/>.</summary>
    public CloseIncidentNotificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.IncidentNotificationId).NotEmpty();
    }
}
