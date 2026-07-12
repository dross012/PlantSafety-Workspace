// ============================================================================
// File: DeleteIncidentRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for DELETE /features/safety-events/incidents/{incidentId},
//              bound from the request body.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Contracts.Requests;

/// <summary>Request body for soft-deleting an Incident.</summary>
public sealed record DeleteIncidentRequest
{
    /// <summary>Reason for deletion, required for audit trail.</summary>
    public required string DeletionReason { get; init; }

    /// <summary>Optimistic concurrency token from the last read of this Incident.</summary>
    public string? ETag { get; init; }
}
