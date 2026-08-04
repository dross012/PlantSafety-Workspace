// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for CharacterizeWasteStreamCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.CharacterizeWasteStreamCommand"/>.</summary>
public sealed class CharacterizeWasteStreamCommandValidator : AbstractValidator<Commands.CharacterizeWasteStreamCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CharacterizeWasteStreamCommand"/>.</summary>
    public CharacterizeWasteStreamCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.StreamName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.EpaWasteCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.HazardClass).NotEmpty();
        RuleFor(x => x.GeneratorEpaId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
