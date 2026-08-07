// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           PackagingSpecificationDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single PackagingSpecification record. Backs
//                  both GetPackagingSpecQuery and ListPackagingSpecsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Models;

/// <summary>A single PackagingSpecification read-model record.</summary>
public sealed record PackagingSpecificationDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PackagingSpecificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Draft/Authorized/Superseded/Retired).</summary>
    public required string Status { get; init; }

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

    /// <summary>HazmatMaterial this packaging spec was authorized for, if applicable.</summary>
    public Guid? AssociatedMaterialId { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
