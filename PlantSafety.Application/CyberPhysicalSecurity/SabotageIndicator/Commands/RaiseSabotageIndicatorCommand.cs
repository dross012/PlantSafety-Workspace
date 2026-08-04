// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to raise a new sabotage indicator, sourced from an unauthorized
//   control change, a failed sensor-integrity check, physical-access anomaly, external intel, or
//   manual analyst input.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Commands;

/// <summary>Raises a new SabotageIndicator.</summary>
public sealed record RaiseSabotageIndicatorCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SiteId,
    string Source,
    string Description,
    string Severity,
    Guid? SourceReferenceId,
    Guid ActorId) : IRequest<Result<RaiseSabotageIndicatorResult>>;
