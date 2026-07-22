// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordAnnualAuditLotoProgramsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordAnnualAuditLotoProgramsCommand"/>.</summary>
public sealed class RecordAnnualAuditLotoProgramsCommandValidator : AbstractValidator<Commands.RecordAnnualAuditLotoProgramsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordAnnualAuditLotoProgramsCommand"/>.</summary>
    public RecordAnnualAuditLotoProgramsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProgramsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}