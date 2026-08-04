// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordDisposalCertificateCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.RecordDisposalCertificateCommand"/>.</summary>
public sealed class RecordDisposalCertificateCommandValidator : AbstractValidator<Commands.RecordDisposalCertificateCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordDisposalCertificateCommand"/>.</summary>
    public RecordDisposalCertificateCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ManifestId).NotEmpty();
        RuleFor(x => x.TsdfFacilityId).NotEmpty();
        RuleFor(x => x.DisposalMethod).NotEmpty();
        RuleFor(x => x.DisposalDateUtc).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
