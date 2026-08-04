// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to formally close an existing CyberPhysicalIncident.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;

/// <summary>Closes an existing CyberPhysicalIncident.</summary>
public sealed record CloseCyberPhysicalIncidentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid CyberPhysicalIncidentId,
    Guid ActorId) : IRequest<Result<CloseCyberPhysicalIncidentResult>>;
