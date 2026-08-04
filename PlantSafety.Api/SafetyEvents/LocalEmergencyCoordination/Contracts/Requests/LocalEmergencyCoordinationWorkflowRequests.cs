// ============================================================================
// File: LocalEmergencyCoordinationWorkflowRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.LocalEmergencyCoordination
// Created: 2026-08-04
// Description: Request DTOs bound from the HTTP request body for the SafetyEvents.
//              LocalEmergencyCoordination V1 controller's write endpoints. Covers the six
//              spec-named workflow commands plus Close/Archive. Every mutating request carries an
//              optional ETag field so clients can supply the optimistic-concurrency token captured
//              from a prior read (e.g. LocalEmergencyCoordinationItemDto.ETag). Mirrors
//              EmergencyManagementLifecycleRequests.cs.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Contracts.Requests;

/// <summary>Request body for establishing a mutual aid agreement.</summary>
public sealed record EstablishMutualAidAgreementRequest
{
    /// <summary>Name of the external agency (fire, police, EMS, mutual-aid partner).</summary>
    public required string AgencyName { get; init; }

    /// <summary>Discipline of the responding agency (Fire/Police/EMS/MutualAid).</summary>
    public required string Discipline { get; init; }

    /// <summary>Agreement document version.</summary>
    public int DocumentVersion { get; init; } = 1;

    /// <summary>UTC instant this agreement version became effective.</summary>
    public required DateTime EffectiveFromUtc { get; init; }

    /// <summary>UTC instant this agreement is next due for review/renewal.</summary>
    public required DateTime RenewalDueAtUtc { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for activating a joint response plan.</summary>
public sealed record ActivateJointResponsePlanRequest
{
    /// <summary>Optional identity of the incident this plan supports.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Descriptive plan name.</summary>
    public required string PlanName { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for deactivating a joint response plan.</summary>
public sealed record DeactivateJointResponsePlanRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for assigning an external responder liaison.</summary>
public sealed record AssignExternalResponderLiaisonRequest
{
    /// <summary>Optional identity of the incident this liaison supports.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Name of the external agency the liaison is assigned to.</summary>
    public required string AgencyName { get; init; }

    /// <summary>Identity of the internal user assigned as liaison.</summary>
    public required Guid LiaisonUserId { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for logging a liaison communication.</summary>
public sealed record LogLiaisonCommunicationRequest
{
    /// <summary>Communication content/summary.</summary>
    public required string Message { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording a joint post-incident debrief.</summary>
public sealed record RecordJointDebriefRequest
{
    /// <summary>Optional identity of the incident this debrief covers.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Agencies that participated in the debrief.</summary>
    public required string AgenciesInvolved { get; init; }

    /// <summary>Debrief summary/findings.</summary>
    public required string Summary { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a LocalEmergencyCoordination case.</summary>
public sealed record CloseLocalEmergencyCoordinationCaseRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed LocalEmergencyCoordination case.</summary>
public sealed record ArchiveLocalEmergencyCoordinationCaseRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this case. Null skips the check.</summary>
    public string? ETag { get; init; }
}
