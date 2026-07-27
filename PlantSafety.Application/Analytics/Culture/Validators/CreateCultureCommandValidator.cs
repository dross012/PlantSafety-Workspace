// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: FluentValidation validator for CreateCultureCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Analytics.Culture.Commands;

namespace PlantSafety.Application.Analytics.Culture.Validators;

/// <summary>Validates <see cref="CreateCultureCommand"/> before it reaches the handler.</summary>
public sealed class CreateCultureCommandValidator : AbstractValidator<CreateCultureCommand>
{
    /// <summary>Configures validation rules for <see cref="CreateCultureCommand"/>.</summary>
    public CreateCultureCommandValidator()
    {
        RuleFor(c => c.TenantId)
            .Must(t => t != default)
            .WithMessage("TenantId must not be empty.");
    }
}
