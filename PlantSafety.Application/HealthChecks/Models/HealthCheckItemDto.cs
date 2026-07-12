// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single recorded health check result.
// SchemaVersion: 1

namespace PlantSafety.Application.HealthChecks.Models;

/// <summary>A single recorded health check execution result.</summary>
public sealed record HealthCheckItemDto
{
    /// <summary>Health check record identifier.</summary>
    public required Guid CheckId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the checked component (e.g., Database, Redis, ServiceBus).</summary>
    public required string CheckName { get; init; }

    /// <summary>Result status (e.g., Healthy/Degraded/Unhealthy).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the check was executed.</summary>
    public required DateTime CheckedAtUtc { get; init; }

    /// <summary>Identity of the process/actor that ran the check.</summary>
    public required Guid CheckedBy { get; init; }
}
