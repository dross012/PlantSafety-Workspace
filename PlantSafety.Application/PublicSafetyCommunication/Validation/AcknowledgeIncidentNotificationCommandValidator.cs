// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for AcknowledgeIncidentNotificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PublicSafetyCommunication.Validation;

/// <summary>Validates <see cref="Commands.AcknowledgeIncidentNotificationCommand"/>.</summary>
public sealed class AcknowledgeIncidentNotificationCommandValidator : AbstractValidator<Commands.AcknowledgeIncidentNotificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AcknowledgeIncidentNotificationCommand"/>.</summary>
    public AcknowledgeIncidentNotificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.IncidentNotificationId).NotEmpty();
        RuleFor(x => x.AcknowledgedByContactName).NotEmpty().MaximumLength(400);
    }
}
