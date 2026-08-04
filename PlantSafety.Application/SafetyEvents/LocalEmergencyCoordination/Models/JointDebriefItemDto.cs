// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           JointDebriefItemDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model item DTO for a single post-incident joint debrief record, backing
//                  the Post-Incident Debrief Summary dashboard and ListJointDebriefsByPeriodQuery.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

/// <summary>A single joint post-incident debrief record tracked by the LocalEmergencyCoordination module.</summary>
public sealed record JointDebriefItemDto
{
    /// <summary>Debrief record identifier.</summary>
    public required Guid JointDebriefRecordId { get; init; }

    /// <summary>Identity of the parent LocalEmergencyCoordination case.</summary>
    public required Guid LocalEmergencyCoordinationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Identity of the incident this debrief covers, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Agencies that participated in the debrief.</summary>
    public required string AgenciesInvolved { get; init; }

    /// <summary>Debrief summary/findings.</summary>
    public required string Summary { get; init; }

    /// <summary>UTC instant the debrief was recorded.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Identity of the user who recorded the debrief.</summary>
    public required Guid RecordedBy { get; init; }
}
