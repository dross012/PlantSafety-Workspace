// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordFitTestPpeCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordFitTestPpeCommand"/>.</summary>
public sealed class RecordFitTestPpeCommandValidator : AbstractValidator<Commands.RecordFitTestPpeCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordFitTestPpeCommand"/>.</summary>
    public RecordFitTestPpeCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PpeId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}