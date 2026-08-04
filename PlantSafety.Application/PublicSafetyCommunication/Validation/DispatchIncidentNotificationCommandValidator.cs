// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DispatchIncidentNotificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PublicSafetyCommunication.Validation;

/// <summary>Validates <see cref="Commands.DispatchIncidentNotificationCommand"/>.</summary>
public sealed class DispatchIncidentNotificationCommandValidator : AbstractValidator<Commands.DispatchIncidentNotificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DispatchIncidentNotificationCommand"/>.</summary>
    public DispatchIncidentNotificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PublicSafetyAgencyId).NotEmpty();
        RuleFor(x => x.DeliveryChannelCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}
