// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for FlagManifestDiscrepancyCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.FlagManifestDiscrepancyCommand"/>.</summary>
public sealed class FlagManifestDiscrepancyCommandValidator : AbstractValidator<Commands.FlagManifestDiscrepancyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagManifestDiscrepancyCommand"/>.</summary>
    public FlagManifestDiscrepancyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardousWasteManifestId).NotEmpty();
        RuleFor(x => x.DiscrepancyReason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
