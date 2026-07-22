// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordDrillEmergencyResponsePlanningCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RecordDrillEmergencyResponsePlanningCommand"/>.</summary>
public sealed class RecordDrillEmergencyResponsePlanningCommandValidator : AbstractValidator<Commands.RecordDrillEmergencyResponsePlanningCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordDrillEmergencyResponsePlanningCommand"/>.</summary>
    public RecordDrillEmergencyResponsePlanningCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyResponsePlanningId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ParticipantCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReadinessScore).InclusiveBetween(0m, 100m);
    }
}