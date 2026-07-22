// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleTestTwoHandControlsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleTestTwoHandControlsCommand"/>.</summary>
public sealed class ScheduleTestTwoHandControlsCommandValidator : AbstractValidator<Commands.ScheduleTestTwoHandControlsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleTestTwoHandControlsCommand"/>.</summary>
    public ScheduleTestTwoHandControlsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TwoHandControlsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}