// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordAssessmentErgonomicsWorkstationDesignCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RecordAssessmentErgonomicsWorkstationDesignCommand"/>.</summary>
public sealed class RecordAssessmentErgonomicsWorkstationDesignCommandValidator : AbstractValidator<Commands.RecordAssessmentErgonomicsWorkstationDesignCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordAssessmentErgonomicsWorkstationDesignCommand"/>.</summary>
    public RecordAssessmentErgonomicsWorkstationDesignCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ErgonomicsWorkstationDesignId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.RiskFactorCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ErgonomicsScore).InclusiveBetween(0m, 100m);
    }
}