// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleAssessmentErgonomicsWorkstationDesignCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.ScheduleAssessmentErgonomicsWorkstationDesignCommand"/>.</summary>
public sealed class ScheduleAssessmentErgonomicsWorkstationDesignCommandValidator : AbstractValidator<Commands.ScheduleAssessmentErgonomicsWorkstationDesignCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleAssessmentErgonomicsWorkstationDesignCommand"/>.</summary>
    public ScheduleAssessmentErgonomicsWorkstationDesignCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ErgonomicsWorkstationDesignId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}