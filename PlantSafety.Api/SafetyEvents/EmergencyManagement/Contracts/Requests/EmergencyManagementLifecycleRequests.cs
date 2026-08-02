// ============================================================================
// File: EmergencyManagementLifecycleRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.EmergencyManagement
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the SafetyEvents.EmergencyManagement
//              V1 controller's write endpoints. Covers the full lifecycle: Declare -> RespondTo ->
//              Stabilize -> Resolve -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              EmergencyManagementItemDto.ETag); a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.EmergencyManagement.Contracts.Requests;

/// <summary>Request body for declaring a new emergency.</summary>
public sealed record DeclareEmergencyRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Emergency classification (e.g., Fire, ChemicalRelease, MedicalEmergency, Evacuation, SevereWeather).</summary>
    public required string EmergencyType { get; init; }

    /// <summary>Emergency severity: Minor, Moderate, Major, Catastrophic.</summary>
    public string Severity { get; init; } = "Minor";

    /// <summary>Physical location the emergency was declared at.</summary>
    public required string Location { get; init; }

    /// <summary>Count of casualties/injuries associated with this emergency.</summary>
    public int CasualtyCount { get; init; }

    /// <summary>Indicates whether the emergency response team has already been notified at declaration time.</summary>
    public bool ResponseTeamNotified { get; init; }
}

/// <summary>Request body for recording that the response team has begun responding (Created -&gt; Active).</summary>
public sealed record RespondToEmergencyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EmergencyManagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording that an active emergency has been stabilized.</summary>
public sealed record StabilizeEmergencyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EmergencyManagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for resolving an active emergency (Active -&gt; Completed).</summary>
public sealed record ResolveEmergencyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EmergencyManagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a resolved emergency record (Completed -&gt; Closed).</summary>
public sealed record CloseEmergencyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EmergencyManagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed emergency record for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveEmergencyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EmergencyManagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
