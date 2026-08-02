// ============================================================================
// File: EnterpriseFabricRequests.cs
// Project: PlantSafety.Api
// Module: EnterpriseFabric
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the EnterpriseFabric V1
//              controller. Covers the full lifecycle: Create -> Activate -> Complete -> Close ->
//              Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.EnterpriseFabric.Contracts.Requests;

/// <summary>Request body for creating a new EnterpriseFabric record.</summary>
public sealed record CreateEnterpriseFabricRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Integration area classification (e.g. IntegrationHub/MasterDataSync/IdentityFederation).</summary>
    public required string FabricArea { get; init; }

    /// <summary>External system/endpoint name.</summary>
    public required string SystemReference { get; init; }

    /// <summary>Integration mechanism (Api/EventBus/FileTransfer/Database).</summary>
    public required string IntegrationType { get; init; }

    /// <summary>Identity of the user who owns this integration record.</summary>
    public required Guid OwnerUserId { get; init; }
}

/// <summary>Request body for activating an EnterpriseFabric record (Created -&gt; Active).</summary>
public sealed record ActivateEnterpriseFabricRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an EnterpriseFabric record (Active -&gt; Completed).</summary>
public sealed record CompleteEnterpriseFabricRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing an EnterpriseFabric record (Completed -&gt; Closed).</summary>
public sealed record CloseEnterpriseFabricRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving an EnterpriseFabric record (Closed -&gt; Archived).</summary>
public sealed record ArchiveEnterpriseFabricRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
