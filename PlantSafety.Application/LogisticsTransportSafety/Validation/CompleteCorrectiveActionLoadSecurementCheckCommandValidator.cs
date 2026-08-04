// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for CompleteCorrectiveActionLoadSecurementCheckCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.CompleteCorrectiveActionLoadSecurementCheckCommand"/>.</summary>
public sealed class CompleteCorrectiveActionLoadSecurementCheckCommandValidator : AbstractValidator<Commands.CompleteCorrectiveActionLoadSecurementCheckCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CompleteCorrectiveActionLoadSecurementCheckCommand"/>.</summary>
    public CompleteCorrectiveActionLoadSecurementCheckCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LoadSecurementCheckId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
