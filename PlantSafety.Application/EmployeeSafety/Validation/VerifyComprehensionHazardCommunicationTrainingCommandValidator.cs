// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for VerifyComprehensionHazardCommunicationTrainingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.VerifyComprehensionHazardCommunicationTrainingCommand"/>.</summary>
public sealed class VerifyComprehensionHazardCommunicationTrainingCommandValidator : AbstractValidator<Commands.VerifyComprehensionHazardCommunicationTrainingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.VerifyComprehensionHazardCommunicationTrainingCommand"/>.</summary>
    public VerifyComprehensionHazardCommunicationTrainingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardCommunicationTrainingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ComprehensionScore).InclusiveBetween(0m, 100m);
    }
}