// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishEmergencyResponsePlanningCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.PublishEmergencyResponsePlanningCommand"/>.</summary>
public sealed class PublishEmergencyResponsePlanningCommandValidator : AbstractValidator<Commands.PublishEmergencyResponsePlanningCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishEmergencyResponsePlanningCommand"/>.</summary>
    public PublishEmergencyResponsePlanningCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyResponsePlanningId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
    }
}