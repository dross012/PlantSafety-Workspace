// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           LocalEmergencyCoordinationItemDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model item DTO for a single LocalEmergencyCoordination case, including
//                  its ETag so callers can read-before-write into a lifecycle command. Mirrors
//                  EmergencyManagementItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

/// <summary>A single local emergency coordination case tracked by the LocalEmergencyCoordination module.</summary>
public sealed record LocalEmergencyCoordinationItemDto
{
    /// <summary>Case identifier.</summary>
    public required Guid LocalEmergencyCoordinationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the SafetyEvents incident this case coordinates external response for, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Descriptive case name.</summary>
    public required string CaseName { get; init; }

    /// <summary>Current case status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the case was opened.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who opened the case.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>UTC timestamp the case was closed, if any.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC timestamp the case was archived, if any.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field.
    /// </summary>
    public string? ETag { get; init; }
}
