// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordAnnualAuditLotoProceduresCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RecordAnnualAuditLotoProceduresCommand"/>.</summary>
public sealed class RecordAnnualAuditLotoProceduresCommandValidator : AbstractValidator<Commands.RecordAnnualAuditLotoProceduresCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordAnnualAuditLotoProceduresCommand"/>.</summary>
    public RecordAnnualAuditLotoProceduresCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProceduresId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m);
    }
}