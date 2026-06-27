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
        RuleFor(x => x.CommandId).IsValidId<CreateBudgetCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.CorrelationId).IsValidId<CreateBudgetCommand, Guid>(x => x.CorrelationId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);

        RuleFor(x => x.AllocatedAmount).MustBePositiveAmount();
        RuleFor(x => x.CurrencyCode).IsValidCurrencyCode();
        RuleFor(x => x.FiscalYear).InclusiveBetween(2000, 2100);
    }
}
