// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: FluentValidation validator for CalculateRoiCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.Financials.Commands.ROI.CalculateROI;
using PlantSafety.Application.Validation.Extensions;

namespace PlantSafety.Application.Features.Financials.Validators.ROI;

/// <summary>Validates <see cref="CalculateRoiCommand"/> structural fields.</summary>
public sealed class CalculateRoiCommandValidator : AbstractValidator<CalculateRoiCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CalculateRoiCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.FiscalYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.TotalInvestment).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.EstimatedAvoidedCosts).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.CurrencyCode).IsValidCurrencyCode();
    }
}
