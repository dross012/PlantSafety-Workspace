// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: FluentValidation validator for RecordSustainabilityMetricCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.Sustainability.Commands.RecordMetric;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.Sustainability.Validators;

/// <summary>Validates RecordSustainabilityMetricCommand structural fields.</summary>
public sealed class RecordSustainabilityMetricCommandValidator : AbstractValidator<RecordSustainabilityMetricCommand>
{
    /// <summary>Defines validation rules.</summary>
    public RecordSustainabilityMetricCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.MetricName).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(CommonValidationRules.MaxReferenceLength);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}
