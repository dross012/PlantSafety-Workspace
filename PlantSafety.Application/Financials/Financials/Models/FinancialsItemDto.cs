// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single financial record.
// SchemaVersion: 1

namespace PlantSafety.Application.Financials.Financials.Models;

/// <summary>A single financial record (cost, budget line, or forecast entry).</summary>
public sealed record FinancialsItemDto
{
    /// <summary>Financial record identifier.</summary>
    public required Guid RecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Financial category (e.g., Budget, Cost, Forecast).</summary>
    public required string Category { get; init; }

    /// <summary>Monetary amount.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Current status (e.g., Draft/Approved/Posted).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the record was recorded.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Identity of the user who recorded the entry.</summary>
    public required Guid RecordedBy { get; init; }
}
