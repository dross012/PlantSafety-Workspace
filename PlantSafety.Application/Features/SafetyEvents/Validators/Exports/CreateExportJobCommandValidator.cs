// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateExportJobCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.Exports.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.Exports;

/// <summary>Validates <see cref="CreateExportJobCommand"/> structural fields.</summary>
public sealed class CreateExportJobCommandValidator : AbstractValidator<CreateExportJobCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CreateExportJobCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<CreateExportJobCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.Format)
            .Must(f => CommonValidationRules.ValidExportFormats.Contains(f))
            .WithMessage($"Format must be one of: {string.Join(", ", CommonValidationRules.ValidExportFormats)}");

        RuleFor(x => x.MaxRecords).InclusiveBetween(1, 10_000);

        RuleFor(x => x.FromUtc).MustBeUtcWhenProvided();
        RuleFor(x => x.ToUtc).MustBeUtcWhenProvided();

        RuleFor(x => x).Must(x =>
            x.FromUtc is null || x.ToUtc is null || x.FromUtc <= x.ToUtc)
            .WithMessage("FromUtc must be before or equal to ToUtc.");
    }
}
