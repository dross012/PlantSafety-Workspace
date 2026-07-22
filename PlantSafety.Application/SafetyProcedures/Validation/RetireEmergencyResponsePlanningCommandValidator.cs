// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireEmergencyResponsePlanningCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RetireEmergencyResponsePlanningCommand"/>.</summary>
public sealed class RetireEmergencyResponsePlanningCommandValidator : AbstractValidator<Commands.RetireEmergencyResponsePlanningCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireEmergencyResponsePlanningCommand"/>.</summary>
    public RetireEmergencyResponsePlanningCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyResponsePlanningId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}