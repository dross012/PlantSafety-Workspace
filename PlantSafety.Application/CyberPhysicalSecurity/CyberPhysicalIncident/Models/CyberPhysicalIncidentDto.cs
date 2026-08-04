// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTOs for the CyberPhysicalIncident subdomain.
// SchemaVersion: 1

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;

/// <summary>Read-model DTO for a single CyberPhysicalIncident record.</summary>
public sealed record CyberPhysicalIncidentDto(
    Guid CyberPhysicalIncidentId,
    Guid TenantId,
    Guid SiteId,
    string Title,
    string Description,
    string Severity,
    string Status,
    IReadOnlyList<Guid> RelatedSabotageIndicatorIds,
    Guid OwnerId,
    DateTime OpenedAtUtc,
    DateTime? ClosedAtUtc,
    int? ResponseTimeMinutes);

/// <summary>Result of a successful OpenCyberPhysicalIncidentCommand.</summary>
public sealed record OpenCyberPhysicalIncidentResult(Guid CyberPhysicalIncidentId, string Status);

/// <summary>Result of a successful CloseCyberPhysicalIncidentCommand.</summary>
public sealed record CloseCyberPhysicalIncidentResult(Guid CyberPhysicalIncidentId, string Status, int ResponseTimeMinutes);
