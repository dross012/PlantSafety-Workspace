// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for SuspendDriverQualificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.SuspendDriverQualificationCommand"/>.</summary>
public sealed class SuspendDriverQualificationCommandValidator : AbstractValidator<Commands.SuspendDriverQualificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SuspendDriverQualificationCommand"/>.</summary>
    public SuspendDriverQualificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DriverQualificationId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
