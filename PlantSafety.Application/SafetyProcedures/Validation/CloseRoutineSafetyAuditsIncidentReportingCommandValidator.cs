// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for CloseRoutineSafetyAuditsIncidentReportingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.CloseRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
public sealed class CloseRoutineSafetyAuditsIncidentReportingCommandValidator : AbstractValidator<Commands.CloseRoutineSafetyAuditsIncidentReportingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CloseRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
    public CloseRoutineSafetyAuditsIncidentReportingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoutineSafetyAuditsIncidentReportingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}