// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ConductRoutineSafetyAuditsIncidentReportingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.ConductRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
public sealed class ConductRoutineSafetyAuditsIncidentReportingCommandValidator : AbstractValidator<Commands.ConductRoutineSafetyAuditsIncidentReportingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ConductRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
    public ConductRoutineSafetyAuditsIncidentReportingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoutineSafetyAuditsIncidentReportingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.FindingCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.ConductedByUserId).NotEmpty();
    }
}