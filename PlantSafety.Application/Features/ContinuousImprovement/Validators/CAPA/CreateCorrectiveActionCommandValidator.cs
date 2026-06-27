// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateCorrectiveActionCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.ContinuousImprovement.Validators.CAPA;

/// <summary>Validates <see cref="CreateCorrectiveActionCommand"/> structural fields.</summary>
public sealed class CreateCorrectiveActionCommandValidator : AbstractValidator<CreateCorrectiveActionCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CreateCorrectiveActionCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<CreateCorrectiveActionCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.CorrelationId).IsValidId<CreateCorrectiveActionCommand, Guid>(x => x.CorrelationId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SourceEventId).NotEmpty();
        RuleFor(x => x.SourceEventType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.AssignedToWorkerId).NotEmpty();
        RuleFor(x => x.DueAtUtc).MustBeUtc();

        RuleFor(x => x.ActionType)
            .Must(t => CommonValidationRules.ValidActionTypes.Contains(t))
            .WithMessage($"ActionType must be one of: {string.Join(", ", CommonValidationRules.ValidActionTypes)}");

        RuleFor(x => x.Priority)
            .Must(p => CommonValidationRules.ValidPriorities.Contains(p))
            .WithMessage($"Priority must be one of: {string.Join(", ", CommonValidationRules.ValidPriorities)}");
    }
}
