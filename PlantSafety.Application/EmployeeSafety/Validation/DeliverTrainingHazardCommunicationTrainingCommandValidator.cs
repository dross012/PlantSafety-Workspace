// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for DeliverTrainingHazardCommunicationTrainingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.DeliverTrainingHazardCommunicationTrainingCommand"/>.</summary>
public sealed class DeliverTrainingHazardCommunicationTrainingCommandValidator : AbstractValidator<Commands.DeliverTrainingHazardCommunicationTrainingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeliverTrainingHazardCommunicationTrainingCommand"/>.</summary>
    public DeliverTrainingHazardCommunicationTrainingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardCommunicationTrainingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.EmployeesTrainedCount).GreaterThanOrEqualTo(0);
    }
}