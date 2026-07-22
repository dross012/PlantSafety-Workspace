// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleRevisionLotoProceduresCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.ScheduleRevisionLotoProceduresCommand"/>.</summary>
public sealed class ScheduleRevisionLotoProceduresCommandValidator : AbstractValidator<Commands.ScheduleRevisionLotoProceduresCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleRevisionLotoProceduresCommand"/>.</summary>
    public ScheduleRevisionLotoProceduresCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProceduresId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}