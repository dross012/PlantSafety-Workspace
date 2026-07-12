// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for AssignTrainingCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.Operational.Commands.Training.AssignTraining;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.Operational.Validators.Training;

/// <summary>Validates <see cref="AssignTrainingCommand"/> structural fields.</summary>
public sealed class AssignTrainingCommandValidator : AbstractValidator<AssignTrainingCommand>
{
    private static readonly string[] ValidDeliveryMethods = ["InPerson", "Online", "OJT"];

    /// <summary>Defines validation rules.</summary>
    public AssignTrainingCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.WorkerId).NotEmpty();
        RuleFor(x => x.TrainingCourseCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TrainingCourseName).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.DueDateUtc).MustBeUtc("DueDateUtc");

        RuleFor(x => x.DeliveryMethod)
            .Must(m => ValidDeliveryMethods.Contains(m))
            .WithMessage($"DeliveryMethod must be one of: {string.Join(", ", ValidDeliveryMethods)}");
    }
}
