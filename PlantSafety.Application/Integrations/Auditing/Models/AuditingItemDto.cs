// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single integration-layer audit log entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Auditing.Models;

/// <summary>A single audit trail entry recording inbound or outbound integration-layer activity with an external system.</summary>
public sealed record AuditingItemDto
{
    /// <summary>Integration audit log entry identifier.</summary>
    public required Guid LogId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the external system involved in the call.</summary>
    public required string ExternalSystem { get; init; }

    /// <summary>Direction of the call (Inbound/Outbound).</summary>
    public required string Direction { get; init; }

    /// <summary>Endpoint or operation invoked.</summary>
    public required string Endpoint { get; init; }

    /// <summary>Outcome status of the call (e.g., Success/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the call occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>Identity of the user or service account that initiated the call.</summary>
    public required Guid InitiatedBy { get; init; }
}
