// ============================================================================
// File: ShippingPapersRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.ShippingPapers
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance.PHMSA.ShippingPapers V1 controller. Covers the full HazmatShipment
//              lifecycle: Create -> GenerateShippingPaper -> MarkReady -> Dispatch/Cancel. Every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              HazmatShipmentDto.ETag); a stale value results in a concurrency-conflict error.
//              Mirrors PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================
namespace PlantSafety.Api.Compliance.PHMSA.ShippingPapers.Contracts.Requests;

/// <summary>Request body for creating a new HazmatShipment record.</summary>
public sealed record CreateHazmatShipmentRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Classified materials contained in this shipment (Classification module). Requires at least one.</summary>
    public required IReadOnlyList<Guid> MaterialIds { get; init; }

    /// <summary>Authorized packaging specifications used for this shipment (Packaging module).</summary>
    public IReadOnlyList<Guid> PackagingIds { get; init; } = [];

    /// <summary>Mode of transport (Ground, Air, Vessel, Rail).</summary>
    public required string Mode { get; init; }
}

/// <summary>Request body for generating and attaching a DOT shipping paper to a HazmatShipment.</summary>
public sealed record GenerateShippingPaperRequest
{
    /// <summary>Optional hazardous-waste manifest reference, when applicable.</summary>
    public string? WasteManifestReference { get; init; }

    /// <summary>Optional shipper certification text override.</summary>
    public string? ShipperCertificationText { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this HazmatShipment record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a HazmatShipment ready for dispatch.</summary>
public sealed record MarkShipmentReadyRequest
{
    /// <summary>Emergency Response Guidebook guide number.</summary>
    public required string ErgGuideNumber { get; init; }

    /// <summary>Emergency contact name.</summary>
    public required string EmergencyContactName { get; init; }

    /// <summary>Emergency contact phone.</summary>
    public required string EmergencyContactPhone { get; init; }

    /// <summary>Emergency response guidance summary.</summary>
    public required string ResponseGuidanceSummary { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this HazmatShipment record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for dispatching a HazmatShipment.</summary>
public sealed record DispatchShipmentRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this HazmatShipment record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for cancelling a HazmatShipment.</summary>
public sealed record CancelShipmentRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this HazmatShipment record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters for the HazmatShipment read-model list endpoint.</summary>
public sealed record HazmatShipmentListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional lifecycle status filter.</summary>
    public string? Status { get; init; }

    /// <summary>Optional transport mode filter.</summary>
    public string? Mode { get; init; }
}
