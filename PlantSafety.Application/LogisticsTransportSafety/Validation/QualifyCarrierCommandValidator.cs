// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for QualifyCarrierCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.QualifyCarrierCommand"/>.</summary>
public sealed class QualifyCarrierCommandValidator : AbstractValidator<Commands.QualifyCarrierCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.QualifyCarrierCommand"/>.</summary>
    public QualifyCarrierCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CarrierId).NotEmpty();
        RuleFor(x => x.SafetyRatingScore).InclusiveBetween(0, 100);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
