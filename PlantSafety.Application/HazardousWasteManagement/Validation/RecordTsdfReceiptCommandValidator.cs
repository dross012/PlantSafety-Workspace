// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordTsdfReceiptCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.RecordTsdfReceiptCommand"/>.</summary>
public sealed class RecordTsdfReceiptCommandValidator : AbstractValidator<Commands.RecordTsdfReceiptCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordTsdfReceiptCommand"/>.</summary>
    public RecordTsdfReceiptCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardousWasteManifestId).NotEmpty();
        RuleFor(x => x.TsdfFacilityId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
