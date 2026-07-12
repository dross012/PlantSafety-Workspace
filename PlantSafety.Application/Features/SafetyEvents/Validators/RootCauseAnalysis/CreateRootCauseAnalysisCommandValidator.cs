// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateRootCauseAnalysisCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.RootCauseAnalysis.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.RootCauseAnalysis;

/// <summary>Validates <see cref="CreateRootCauseAnalysisCommand"/> structural fields.</summary>
public sealed class CreateRootCauseAnalysisCommandValidator : AbstractValidator<CreateRootCauseAnalysisCommand>
{
    private static readonly string[] ValidMethodologies = ["5Why", "FishboneDiagram", "FaultTree"];

    /// <summary>Defines validation rules.</summary>
    public CreateRootCauseAnalysisCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.LinkedEventId).NotEmpty();
        RuleFor(x => x.LinkedEventType).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Methodology)
            .Must(m => ValidMethodologies.Contains(m))
            .WithMessage($"Methodology must be one of: {string.Join(", ", ValidMethodologies)}");

        RuleFor(x => x.RootCauses).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.CorrectiveActions).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
    }
}
