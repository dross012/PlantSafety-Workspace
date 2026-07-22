// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
public sealed class BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommandValidator : AbstractValidator<Commands.BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
    public BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoutineSafetyAuditsIncidentReportingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}