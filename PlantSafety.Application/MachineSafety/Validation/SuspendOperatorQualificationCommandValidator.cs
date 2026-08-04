// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for SuspendOperatorQualificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.SuspendOperatorQualificationCommand"/>.</summary>
public sealed class SuspendOperatorQualificationCommandValidator : AbstractValidator<Commands.SuspendOperatorQualificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SuspendOperatorQualificationCommand"/>.</summary>
    public SuspendOperatorQualificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CraneHoistSystemsId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
