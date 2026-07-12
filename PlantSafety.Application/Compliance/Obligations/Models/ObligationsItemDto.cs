// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single compliance obligation record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Obligations.Models;

/// <summary>A single compliance obligation owed to a regulatory body.</summary>
public sealed record ObligationsItemDto
{
    /// <summary>Obligation identifier.</summary>
    public required Guid ObligationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the obligation.</summary>
    public required string ObligationTitle { get; init; }

    /// <summary>Regulatory body the obligation is owed to.</summary>
    public required string RegulatoryBody { get; init; }

    /// <summary>Current obligation status (e.g., Open/Fulfilled/Overdue).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the obligation is due.</summary>
    public required DateTime DueAtUtc { get; init; }

    /// <summary>Identity of the user who owns the obligation.</summary>
    public required Guid OwnerId { get; init; }
}
