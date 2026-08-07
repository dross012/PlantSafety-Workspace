// ============================================================================
// File: PackagingRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Packaging
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance.PHMSA.Packaging V1 controller. Every mutating lifecycle request carries
//              an optional ETag field so clients can supply the optimistic-concurrency token
//              captured from a prior read (e.g. PackagingSpecificationDto.ETag); a stale value
//              results in a concurrency-conflict error. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================

namespace PlantSafety.Api.Compliance.PHMSA.Packaging.Contracts.Requests;

/// <summary>Request body for authorizing a new DOT packaging specification.</summary>
public sealed record AuthorizePackagingRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>DOT packaging specification code, e.g. "1A1", "3A", "4G".</summary>
    public required string DOTSpec { get; init; }

    /// <summary>Performance tests this packaging has passed.</summary>
    public IReadOnlyList<string> PerformanceTests { get; init; } = [];

    /// <summary>Maximum rated capacity.</summary>
    public required decimal MaxCapacity { get; init; }

    /// <summary>Unit of measure for <see cref="MaxCapacity"/>, e.g. "L", "kg".</summary>
    public required string CapacityUnit { get; init; }

    /// <summary>Closure instructions required for this packaging to remain compliant.</summary>
    public required string ClosureInstructions { get; init; }

    /// <summary>Restrictions on reuse of this packaging, if any.</summary>
    public string? ReuseRestrictions { get; init; }

    /// <summary>HazmatMaterial this packaging spec is authorized for, if applicable.</summary>
    public Guid? AssociatedMaterialId { get; init; }
}

/// <summary>Request body for superseding a PackagingSpecification with a newer authorization.</summary>
public sealed record SupersedePackagingRequest
{
    /// <summary>Identity of the PackagingSpecification that supersedes this authorization.</summary>
    public required Guid SupersededBySpecificationId { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for retiring a PackagingSpecification record.</summary>
public sealed record RetirePackagingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for validating a proposed DOT packaging spec against a hazard class/packing group.</summary>
public sealed record ValidatePackagingRequest
{
    /// <summary>DOT packaging specification code being validated, e.g. "1A1".</summary>
    public required string ProposedDOTSpec { get; init; }

    /// <summary>Primary DOT hazard class, 1-9.</summary>
    public required int HazardClass { get; init; }

    /// <summary>Optional division within the class (e.g. "1" in Class 1.1).</summary>
    public string? HazardClassDivision { get; init; }

    /// <summary>DOT packing group (I/II/III/NotApplicable).</summary>
    public required string PackingGroup { get; init; }
}

/// <summary>Query-string bound filters for the PackagingSpecification read-model list endpoint.</summary>
public sealed record PackagingListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional DOT packaging specification code filter.</summary>
    public string? DOTSpec { get; init; }
}

/// <summary>Query-string bound parameters for the packaging suggestion endpoint.</summary>
public sealed record SuggestPackagingRequest
{
    /// <summary>Primary DOT hazard class, 1-9.</summary>
    public required int HazardClass { get; init; }

    /// <summary>Optional division within the class (e.g. "1" in Class 1.1).</summary>
    public string? HazardClassDivision { get; init; }

    /// <summary>DOT packing group (I/II/III/NotApplicable).</summary>
    public required string PackingGroup { get; init; }

    /// <summary>Quantity of material to be packaged.</summary>
    public required decimal Quantity { get; init; }

    /// <summary>Unit of measure for <see cref="Quantity"/>.</summary>
    public required string QuantityUnit { get; init; }

    /// <summary>Requested container type/form factor.</summary>
    public required string ContainerTypeRequested { get; init; }
}
