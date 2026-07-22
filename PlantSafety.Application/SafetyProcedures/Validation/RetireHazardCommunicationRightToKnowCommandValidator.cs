// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireHazardCommunicationRightToKnowCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RetireHazardCommunicationRightToKnowCommand"/>.</summary>
public sealed class RetireHazardCommunicationRightToKnowCommandValidator : AbstractValidator<Commands.RetireHazardCommunicationRightToKnowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireHazardCommunicationRightToKnowCommand"/>.</summary>
    public RetireHazardCommunicationRightToKnowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardCommunicationRightToKnowId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}