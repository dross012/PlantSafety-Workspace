// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Shared base type providing common audit and concurrency fields used by
//   Application-layer DTO and read-model contracts.  Do NOT inherit in Domain entities.
// SchemaVersion: 1

namespace PlantSafety.Application.Common.Types;

/// <summary>
/// Base record carrying canonical audit and concurrency fields shared by Application DTOs
/// and read-model projections.  Not intended for Domain entity inheritance.
/// </summary>
public abstract record EntityBase
{
    /// <summary>Tenant identifier.  Mandatory on every entity; enforces multi-tenancy.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Actor identifier of the user who last modified this record.</summary>
    public required Guid LastModifiedByActorId { get; init; }

    /// <summary>UTC timestamp of the last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Opaque row-version token used for optimistic concurrency (ETag).</summary>
    public required string ETag { get; init; }
}
