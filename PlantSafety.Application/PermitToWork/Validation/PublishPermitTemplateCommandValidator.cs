// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishPermitTemplateCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.PublishPermitTemplateCommand"/>.</summary>
public sealed class PublishPermitTemplateCommandValidator : AbstractValidator<Commands.PublishPermitTemplateCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishPermitTemplateCommand"/>.</summary>
    public PublishPermitTemplateCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitTemplateId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
