// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for NotifySupervisorOfDetectionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Observability.Validation;

/// <summary>Validates <see cref="Commands.NotifySupervisorOfDetectionCommand"/>.</summary>
public sealed class NotifySupervisorOfDetectionCommandValidator : AbstractValidator<Commands.NotifySupervisorOfDetectionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.NotifySupervisorOfDetectionCommand"/>.</summary>
    public NotifySupervisorOfDetectionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutomatedIncidentDetectionId).NotEmpty();
        RuleFor(x => x.SupervisorUserId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
