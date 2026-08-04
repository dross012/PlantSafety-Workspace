// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for QualifyDriverCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.QualifyDriverCommand"/>.</summary>
public sealed class QualifyDriverCommandValidator : AbstractValidator<Commands.QualifyDriverCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.QualifyDriverCommand"/>.</summary>
    public QualifyDriverCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DriverUserId).NotEmpty();
        RuleFor(x => x.QualificationType).NotEmpty();
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ExpiresAtUtc).GreaterThan(x => x.IssuedAtUtc);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
