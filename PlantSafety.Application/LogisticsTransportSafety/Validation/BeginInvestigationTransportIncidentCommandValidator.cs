// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for BeginInvestigationTransportIncidentCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.BeginInvestigationTransportIncidentCommand"/>.</summary>
public sealed class BeginInvestigationTransportIncidentCommandValidator : AbstractValidator<Commands.BeginInvestigationTransportIncidentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginInvestigationTransportIncidentCommand"/>.</summary>
    public BeginInvestigationTransportIncidentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransportIncidentId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
