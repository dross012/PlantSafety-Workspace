// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ExpireSiteOrientationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.ExpireSiteOrientationCommand"/>.</summary>
public sealed class ExpireSiteOrientationCommandValidator : AbstractValidator<Commands.ExpireSiteOrientationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ExpireSiteOrientationCommand"/>.</summary>
    public ExpireSiteOrientationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteOrientationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}