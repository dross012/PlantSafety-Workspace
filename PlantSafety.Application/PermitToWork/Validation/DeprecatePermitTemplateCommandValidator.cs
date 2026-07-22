// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for DeprecatePermitTemplateCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.DeprecatePermitTemplateCommand"/>.</summary>
public sealed class DeprecatePermitTemplateCommandValidator : AbstractValidator<Commands.DeprecatePermitTemplateCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeprecatePermitTemplateCommand"/>.</summary>
    public DeprecatePermitTemplateCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitTemplateId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
