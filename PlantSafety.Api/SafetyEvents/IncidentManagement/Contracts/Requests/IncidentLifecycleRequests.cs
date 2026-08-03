// ============================================================================
// File: IncidentLifecycleRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: External-facing request DTOs for the Incident lifecycle actions exposed on
//              IncidentsCanonicalV1Controller (assign/escalate/notes/resolve). Field names
//              mirror PlantSafety.UI.Modules.IncidentManagement.ApiClients.
//              IncidentManagementApiClient's anonymous POST payloads exactly.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.IncidentManagement.Contracts.Requests;

/// <summary>Request body for POST /safety-events/incidents/{id}/assign.</summary>
public sealed record AssignTechnicianRequest
{
    /// <summary>Technician/investigator to assign.</summary>
    public required Guid TechnicianId { get; init; }
}

/// <summary>Request body for POST /safety-events/incidents/{id}/escalate.</summary>
public sealed record EscalateIncidentRequest
{
    /// <summary>Reason for the escalation.</summary>
    public required string Reason { get; init; }
}

/// <summary>Request body for POST /safety-events/incidents/{id}/notes.</summary>
public sealed record AddIncidentNoteRequest
{
    /// <summary>Note text.</summary>
    public required string Text { get; init; }
}

/// <summary>Request body for POST /safety-events/incidents/{id}/resolve.</summary>
public sealed record ResolveIncidentRequest
{
    /// <summary>Resolution notes.</summary>
    public required string ResolutionNotes { get; init; }
}
