// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleRevisionHazardCommunicationRightToKnowCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.ScheduleRevisionHazardCommunicationRightToKnowCommand"/>.</summary>
public sealed class ScheduleRevisionHazardCommunicationRightToKnowCommandValidator : AbstractValidator<Commands.ScheduleRevisionHazardCommunicationRightToKnowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleRevisionHazardCommunicationRightToKnowCommand"/>.</summary>
    public ScheduleRevisionHazardCommunicationRightToKnowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardCommunicationRightToKnowId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}