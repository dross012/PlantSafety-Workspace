// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PPEInventory record. PPE inventory and lifecycle management: stock, issuance, inspection intervals, and expiration dates.
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.PPEInventory.Models;

/// <summary>A single PPEInventory read-model record.</summary>
public sealed record PPEInventoryItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PPEInventoryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>PpeItemCode.</summary>
    public required  string PpeItemCode { get; init; }

    /// <summary>StockQuantity.</summary>
    public required  int StockQuantity { get; init; }

    /// <summary>IssuedQuantity.</summary>
    public required  int IssuedQuantity { get; init; }

    /// <summary>ExpiresAtUtc.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}