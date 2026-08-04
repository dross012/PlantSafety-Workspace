// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single CyberPhysicalIncident read-model record by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Queries;

/// <summary>Retrieves a single CyberPhysicalIncident read-model record by identity.</summary>
public sealed record GetCyberPhysicalIncidentByIdQuery(Guid TenantId, Guid CyberPhysicalIncidentId) : IRequest<Result<CyberPhysicalIncidentDto>>;
