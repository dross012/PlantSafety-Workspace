// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to open a new CyberPhysicalIncident (Incident Response subdomain).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;

/// <summary>Opens a new CyberPhysicalIncident.</summary>
public sealed record OpenCyberPhysicalIncidentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SiteId,
    string Title,
    string Description,
    string Severity,
    Guid OwnerId,
    IReadOnlyList<Guid>? RelatedSabotageIndicatorIds,
    Guid ActorId) : IRequest<Result<OpenCyberPhysicalIncidentResult>>;
