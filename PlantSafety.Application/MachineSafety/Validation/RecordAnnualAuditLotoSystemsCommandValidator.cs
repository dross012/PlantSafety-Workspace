// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordAnnualAuditLotoSystemsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordAnnualAuditLotoSystemsCommand"/>.</summary>
public sealed class RecordAnnualAuditLotoSystemsCommandValidator : AbstractValidator<Commands.RecordAnnualAuditLotoSystemsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordAnnualAuditLotoSystemsCommand"/>.</summary>
    public RecordAnnualAuditLotoSystemsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoSystemsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m);
    }
}