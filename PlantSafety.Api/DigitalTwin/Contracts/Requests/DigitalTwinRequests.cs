// ============================================================================
// File: DigitalTwinRequests.cs
// Project: PlantSafety.Api
// Module: DigitalTwin
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the DigitalTwin V1 controller.
//              Covers the full lifecycle: Create -> Activate -> RefreshSync -> Complete -> Close
//              -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.DigitalTwin.Contracts.Requests;

/// <summary>Request body for registering a new DigitalTwin model registry record.</summary>
public sealed record CreateDigitalTwinRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the digital twin.</summary>
    public required string TwinName { get; init; }

    /// <summary>Identity of the physical asset this twin represents, if known.</summary>
    public Guid? AssetReferenceId { get; init; }

    /// <summary>Type of the underlying digital-twin model.</summary>
    public required string ModelType { get; init; }
}

/// <summary>Request body for activating a DigitalTwin record (Created -&gt; Active).</summary>
public sealed record ActivateDigitalTwinRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for refreshing a DigitalTwin's last-synchronized timestamp (same-status action while Active).</summary>
public sealed record RefreshSyncDigitalTwinRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing a DigitalTwin record (Active -&gt; Completed).</summary>
public sealed record CompleteDigitalTwinRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a DigitalTwin record (Completed -&gt; Closed).</summary>
public sealed record CloseDigitalTwinRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a DigitalTwin record (Closed -&gt; Archived).</summary>
public sealed record ArchiveDigitalTwinRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
