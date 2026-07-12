// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateBudgetCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.Financials.Commands.Budgets.CreateBudget;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.Financials.Validators.Budgets;

/// <summary>Validates <see cref="CreateBudgetCommand"/> structural fields.</summary>
public sealed class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.Category).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);

        RuleFor(x => x.AllocatedAmount).MustBePositiveAmount("AllocatedAmount");
        RuleFor(x => x.CurrencyCode).IsValidCurrencyCode();
        RuleFor(x => x.FiscalYear).InclusiveBetween(2000, 2100);
    }
}
