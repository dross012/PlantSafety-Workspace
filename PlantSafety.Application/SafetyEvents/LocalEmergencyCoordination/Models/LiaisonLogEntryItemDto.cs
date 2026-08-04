// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           LiaisonLogEntryItemDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model item DTO for a single liaison communication log entry, backing
//                  GetLiaisonLogByIncidentQuery.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

/// <summary>A single liaison communication log entry tracked by the LocalEmergencyCoordination module.</summary>
public sealed record LiaisonLogEntryItemDto
{
    /// <summary>Log entry identifier.</summary>
    public required Guid LiaisonLogEntryId { get; init; }

    /// <summary>Identity of the parent ExternalResponderLiaison assignment.</summary>
    public required Guid ExternalResponderLiaisonId { get; init; }

    /// <summary>Identity of the parent LocalEmergencyCoordination case.</summary>
    public required Guid LocalEmergencyCoordinationId { get; init; }

    /// <summary>Identity of the incident this liaison supports, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Name of the external agency the liaison is assigned to.</summary>
    public required string AgencyName { get; init; }

    /// <summary>Communication content/summary.</summary>
    public required string Message { get; init; }

    /// <summary>UTC instant the communication was logged.</summary>
    public required DateTime LoggedAtUtc { get; init; }

    /// <summary>Identity of the user who logged the communication.</summary>
    public required Guid LoggedBy { get; init; }
}
