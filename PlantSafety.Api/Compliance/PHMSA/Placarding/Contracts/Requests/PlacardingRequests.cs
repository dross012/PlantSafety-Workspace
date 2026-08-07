// ============================================================================
// File: PlacardingRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Placarding
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance.PHMSA.Placarding V1 controller. Every mutating lifecycle request
//              carries an optional ETag field so clients can supply the optimistic-concurrency
//              token captured from a prior read (e.g. HazmatContainerDto.ETag); a stale value
//              results in a concurrency-conflict error. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

namespace PlantSafety.Api.Compliance.PHMSA.Placarding.Contracts.Requests;

/// <summary>Request body for creating a new HazmatContainer record.</summary>
public sealed record CreateHazmatContainerRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Physical container/packaging form factor (e.g. Drum, IntermediateBulkContainer, Cylinder).</summary>
    public required string ContainerType { get; init; }

    /// <summary>Reference to the classified material inside this container (Classification module).</summary>
    public required Guid MaterialId { get; init; }

    /// <summary>Reference to the authorized packaging specification (Packaging module).</summary>
    public required Guid PackagingSpecId { get; init; }
}

/// <summary>Request body for applying markings and hazard labels to a HazmatContainer.</summary>
public sealed record ApplyMarkingsAndLabelsRequest
{
    /// <summary>Text markings applied.</summary>
    public IReadOnlyList<string> Markings { get; init; } = [];

    /// <summary>Hazard label names.</summary>
    public IReadOnlyList<string> Labels { get; init; } = [];

    /// <summary>Whether orientation arrows are required on this container's packaging.</summary>
    public bool OrientationArrowsRequired { get; init; }

    /// <summary>Explosives compatibility group letter (A-N, S). Only meaningful for Class 1 materials.</summary>
    public string? CompatibilityGroup { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this HazmatContainer record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for computing and recording the placard set for a HazmatContainer.</summary>
public sealed record ApplyPlacardsRequest
{
    /// <summary>The hazard classes present in the load, with quantity and unit.</summary>
    public required IReadOnlyList<PlacardingMaterialItem> Materials { get; init; }

    /// <summary>Whether the load is carried in bulk packaging.</summary>
    public bool IsBulkPackaging { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this HazmatContainer record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for retiring a HazmatContainer record.</summary>
public sealed record RetireHazmatContainerRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this HazmatContainer record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for computing placards with no persistence.</summary>
public sealed record CalculatePlacardsRequest
{
    /// <summary>The hazard classes present in the load, with quantity and unit.</summary>
    public required IReadOnlyList<PlacardingMaterialItem> Materials { get; init; }

    /// <summary>Whether the load is carried in bulk packaging.</summary>
    public bool IsBulkPackaging { get; init; }
}

/// <summary>Request body for validating placarding against an existing HazmatShipment.</summary>
public sealed record ValidatePlacardingForShipmentRequest
{
    /// <summary>Whether the shipment's load is carried in bulk packaging.</summary>
    public bool IsBulkPackaging { get; init; }
}

/// <summary>Query-string bound filters for the HazmatContainer read-model list endpoint.</summary>
public sealed record HazmatContainerListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional material filter.</summary>
    public Guid? MaterialId { get; init; }
}
