// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RetireDriverQualificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.RetireDriverQualificationCommand"/>.</summary>
public sealed class RetireDriverQualificationCommandValidator : AbstractValidator<Commands.RetireDriverQualificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireDriverQualificationCommand"/>.</summary>
    public RetireDriverQualificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DriverQualificationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
