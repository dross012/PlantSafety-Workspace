// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for GenerateManifestCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.GenerateManifestCommand"/>.</summary>
public sealed class GenerateManifestCommandValidator : AbstractValidator<Commands.GenerateManifestCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.GenerateManifestCommand"/>.</summary>
    public GenerateManifestCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ManifestNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.GeneratorEpaId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WasteStreamId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
