// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for OpenCyberPhysicalIncidentCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Validation;

/// <summary>Validates <see cref="Commands.OpenCyberPhysicalIncidentCommand"/>.</summary>
public sealed class OpenCyberPhysicalIncidentCommandValidator : AbstractValidator<Commands.OpenCyberPhysicalIncidentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.OpenCyberPhysicalIncidentCommand"/>.</summary>
    public OpenCyberPhysicalIncidentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Severity).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
