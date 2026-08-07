// ============================================================================
// File: HazmatClassificationRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Classification
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance.PHMSA.Classification V1 controller. Every mutating lifecycle request
//              carries an optional ETag field so clients can supply the optimistic-concurrency
//              token captured from a prior read (e.g. HazmatMaterialDto.ETag); a stale value
//              results in a concurrency-conflict error. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================

namespace PlantSafety.Api.Compliance.PHMSA.Classification.Contracts.Requests;

/// <summary>Request body for classifying a candidate hazardous material.</summary>
public sealed record ClassifyMaterialRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable candidate material name.</summary>
    public required string MaterialName { get; init; }

    /// <summary>CAS registry number for the candidate material, if known.</summary>
    public string? CasNumber { get; init; }

    /// <summary>Waste profile code for the candidate material, if applicable.</summary>
    public string? WasteProfileCode { get; init; }

    /// <summary>Safety Data Sheet hazard statement text, if available.</summary>
    public string? SdsHazardStatement { get; init; }
}

/// <summary>Request body for superseding a HazmatMaterial classification with a newer one.</summary>
public sealed record SupersedeHazmatMaterialRequest
{
    /// <summary>Identity of the HazmatMaterial that supersedes this classification.</summary>
    public required Guid SupersededByMaterialId { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for retiring a HazmatMaterial classification record.</summary>
public sealed record RetireHazmatMaterialRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters for the HazmatMaterial read-model list endpoint.</summary>
public sealed record ListClassifiedMaterialsRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional DOT/UN number filter.</summary>
    public string? UNNumber { get; init; }
}
