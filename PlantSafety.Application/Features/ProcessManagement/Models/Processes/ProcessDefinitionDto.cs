// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Read-side DTO for a Process Definition record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ProcessManagement.Models.Processes;

/// <summary>
/// Read-side projection of a ProcessDefinition aggregate.
/// Process definitions track documented manufacturing/operational process revisions
/// and their safety-critical parameters across sites.
/// </summary>
public sealed record ProcessDefinitionDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid ProcessId { get; init; }

    /// <summary>Tenant owning this process definition.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this process is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable process name.</summary>
    public required string ProcessName { get; init; }

    /// <summary>Safety-critical process category.</summary>
    public required string Category { get; init; }

    /// <summary>Lifecycle status (Draft, Active, UnderReview, Retired).</summary>
    public required string Status { get; init; }

    /// <summary>Monotonically increasing revision number.</summary>
    public required int Version { get; init; }

    /// <summary>Identity of the accountable process owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
