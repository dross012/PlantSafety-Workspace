// ============================================================================
// File: OpenLocalEmergencyCoordinationCaseRequest.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.LocalEmergencyCoordination
// Created: 2026-08-04
// Description: Request body for POST /safety-events/local-emergency-coordination, opening a new
//              coordination case. Mirrors DeclareEmergencyRequest.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Contracts.Requests;

/// <summary>Request body for opening a new LocalEmergencyCoordination case.</summary>
public sealed record OpenLocalEmergencyCoordinationCaseRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional identity of the SafetyEvents incident (EmergencyManagement/CrisisManagement) this case coordinates external response for.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Descriptive name for this coordination case.</summary>
    public required string CaseName { get; init; }
}
