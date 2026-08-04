// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for ReinstateDriverQualificationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.ReinstateDriverQualificationCommand"/>.</summary>
public sealed class ReinstateDriverQualificationCommandValidator : AbstractValidator<Commands.ReinstateDriverQualificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ReinstateDriverQualificationCommand"/>.</summary>
    public ReinstateDriverQualificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DriverQualificationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
